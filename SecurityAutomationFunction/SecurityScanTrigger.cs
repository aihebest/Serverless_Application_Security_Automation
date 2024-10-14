using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Network;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Compute.Models;
using Azure.ResourceManager.Storage.Models;
using Microsoft.Azure.Cosmos;
using SecurityAutomationFunction;

namespace SecurityAutomationFunction
{
    public static class SecurityScanTrigger
    {
        private static readonly string EndpointUri = Environment.GetEnvironmentVariable("CosmosDBEndpointUrl");
        private static readonly string PrimaryKey = Environment.GetEnvironmentVariable("CosmosDBPrimaryKey");
        private static CosmosClient cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);

        [FunctionName("SecurityScanTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string resourceId = req.Query["resourceId"];
            string scanType = req.Query["scanType"];

            if (string.IsNullOrEmpty(resourceId))
            {
                return new BadRequestObjectResult("Please pass a resourceId on the query string.");
            }

            try
            {
                var credential = new DefaultAzureCredential();
                var armClient = new ArmClient(credential);

                var resourceIdentifier = new ResourceIdentifier(resourceId);
                var scanResult = await ScanResourceAsync(armClient, resourceIdentifier, scanType, log);

                // Store the scan result in Cosmos DB
                await StoreScanResultAsync(scanResult);

                return new OkObjectResult(scanResult);
            }
            catch (Exception ex)
            {
                log.LogError($"Error scanning resource: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        private static async Task<ScanResult> ScanResourceAsync(ArmClient armClient, ResourceIdentifier resourceIdentifier, string scanType, ILogger log)
        {
            switch (resourceIdentifier.ResourceType.ToString())
            {
                case "Microsoft.Compute/virtualMachines":
                    return await ScanVirtualMachineAsync(armClient, resourceIdentifier, scanType, log);
                case "Microsoft.Storage/storageAccounts":
                    return await ScanStorageAccountAsync(armClient, resourceIdentifier, scanType, log);
                default:
                    throw new NotSupportedException($"Scanning for resource type {resourceIdentifier.ResourceType} is not supported.");
            }
        }

        private static async Task<ScanResult> ScanVirtualMachineAsync(ArmClient armClient, ResourceIdentifier resourceIdentifier, string scanType, ILogger log)
        {
            var virtualMachine = armClient.GetVirtualMachineResource(resourceIdentifier);
            var vmData = await virtualMachine.GetAsync();

            var findings = new List<Finding>();

            // Check for managed disks
            if (vmData.Value.Data.StorageProfile.OsDisk.ManagedDisk == null)
            {
                findings.Add(new Finding("High", "VM is not using managed disks", "Convert to managed disks for improved security and management"));
            }

            // Check OS version
            if (vmData.Value.Data.StorageProfile.ImageReference != null &&
                vmData.Value.Data.StorageProfile.ImageReference.Offer == "WindowsServer" &&
                vmData.Value.Data.StorageProfile.ImageReference.Sku != "2019-Datacenter")
            {
                findings.Add(new Finding("Medium", "VM is not using the latest OS version", "Update to the latest OS version to ensure all security patches are applied"));
            }

            return new ScanResult
            {
                ResourceId = virtualMachine.Id.ToString(),
                ScanType = scanType,
                Timestamp = DateTime.UtcNow,
                Findings = findings
            };
        }

        private static async Task<ScanResult> ScanStorageAccountAsync(ArmClient armClient, ResourceIdentifier resourceIdentifier, string scanType, ILogger log)
        {
            var storageAccount = armClient.GetStorageAccountResource(resourceIdentifier);
            var storageData = await storageAccount.GetAsync();

            var findings = new List<Finding>();

            // Check for public access
            if (storageData.Value.Data.PublicNetworkAccess == PublicNetworkAccess.Enabled)
            {
                findings.Add(new Finding("High", "Storage account allows public access", "Consider disabling public access and use private endpoints"));
            }

            // Check for encryption
            if (storageData.Value.Data.Encryption.KeySource == StorageAccountKeySource.Microsoft)
            {
                findings.Add(new Finding("Medium", "Storage account is using Microsoft-managed keys", "Consider using customer-managed keys for added security"));
            }

            return new ScanResult
            {
                ResourceId = storageAccount.Id.ToString(),
                ScanType = scanType,
                Timestamp = DateTime.UtcNow,
                Findings = findings
            };
        }

        private static async Task StoreScanResultAsync(ScanResult scanResult)
        {
            var database = cosmosClient.GetDatabase("SecurityScans");
            var container = database.GetContainer("ScanResults");

            await container.CreateItemAsync(scanResult, new PartitionKey(scanResult.ResourceId));
        }
    }

    public class ScanResult
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ResourceId { get; set; }
        public string ScanType { get; set; }
        public DateTime Timestamp { get; set; }
        public List<Finding> Findings { get; set; }
    }

    public class Finding
    {
        public string Severity { get; set; }
        public string Description { get; set; }
        public string Recommendation { get; set; }

        public Finding(string severity, string description, string recommendation)
        {
            Severity = severity;
            Description = description;
            Recommendation = recommendation;
        }
    }
}