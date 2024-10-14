using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Microsoft.Azure.Management.CosmosDB;
using Microsoft.Azure.Management.CosmosDB.Models;
using Microsoft.Rest;

namespace SecurityScanFunction
{
    public static class SecurityScanTrigger
    {
        [FunctionName("SecurityScanTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string resourceId = req.Query["resourceId"];

            if (string.IsNullOrEmpty(resourceId))
            {
                return new BadRequestObjectResult("Please pass a resourceId on the query string.");
            }

            var findings = new List<Finding>();

            try
            {
                var credential = new DefaultAzureCredential();
                var token = await credential.GetTokenAsync(new Azure.Core.TokenRequestContext(new[] { "https://management.azure.com/.default" }));
                var tokenCredentials = new TokenCredentials(token.Token);

                var cosmosDbManagementClient = new CosmosDBManagementClient(tokenCredentials);
                cosmosDbManagementClient.SubscriptionId = ExtractSubscriptionId(resourceId);

                var accountName = ExtractAccountName(resourceId);
                var resourceGroupName = ExtractResourceGroupName(resourceId);

                var account = await cosmosDbManagementClient.DatabaseAccounts.GetAsync(resourceGroupName, accountName);

                // Check for public network access
                if (account.PublicNetworkAccess == "Enabled")
                {
                    findings.Add(new Finding { Severity = "High", Description = "Cosmos DB account allows public access", Recommendation = "Consider disabling public access and use private endpoints" });
                }

                // Check for encryption at rest (customer-managed keys)
                if (string.IsNullOrEmpty(account.KeyVaultKeyUri))
                {
                    findings.Add(new Finding { Severity = "High", Description = "Cosmos DB account is not using customer-managed keys for encryption", Recommendation = "Enable customer-managed keys for enhanced security" });
                }

                // Check for firewall rules
                if (account.IpRules == null || account.IpRules.Count == 0)
                {
                    findings.Add(new Finding { Severity = "Medium", Description = "No IP firewall rules configured", Recommendation = "Configure IP firewall rules to restrict access" });
                }

                // Check for automatic failover
                if (!account.EnableAutomaticFailover.GetValueOrDefault())
                {
                    findings.Add(new Finding { Severity = "Low", Description = "Automatic failover is not enabled", Recommendation = "Enable automatic failover for improved availability" });
                }

                // Check for multi-region writes
                if (!account.EnableMultipleWriteLocations.GetValueOrDefault())
                {
                    findings.Add(new Finding { Severity = "Low", Description = "Multi-region writes are not enabled", Recommendation = "Consider enabling multi-region writes for improved performance and availability" });
                }

                var scanResult = new ScanResult
                {
                    Id = Guid.NewGuid().ToString(),
                    ResourceId = resourceId,
                    ResourceName = account.Name,
                    ScanTime = DateTime.UtcNow,
                    Findings = findings
                };

                return new OkObjectResult(scanResult);
            }
            catch (Exception ex)
            {
                log.LogError($"Error: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        private static string ExtractSubscriptionId(string resourceId)
        {
            var parts = resourceId.Split('/');
            return parts.Length > 2 ? parts[2] : string.Empty;
        }

        private static string ExtractResourceGroupName(string resourceId)
        {
            var parts = resourceId.Split('/');
            return parts.Length > 4 ? parts[4] : string.Empty;
        }

        private static string ExtractAccountName(string resourceId)
        {
            var parts = resourceId.Split('/');
            return parts.Length > 8 ? parts[8] : string.Empty;
        }
    }
}