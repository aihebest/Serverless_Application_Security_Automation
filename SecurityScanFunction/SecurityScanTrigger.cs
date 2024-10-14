using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.CosmosDB;

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

            try
            {
                var credential = new DefaultAzureCredential();
                var armClient = new ArmClient(credential);

                var resourceIdentifier = new Azure.Core.ResourceIdentifier(resourceId);
                var cosmosDBAccount = armClient.GetCosmosDBAccountResource(resourceIdentifier);
                var accountInfo = await cosmosDBAccount.GetAsync();

                var findings = new System.Collections.Generic.List<object>();

                // Check if public network access is enabled
                if (accountInfo.Value.Data.PublicNetworkAccess.ToString().Equals("Enabled", StringComparison.OrdinalIgnoreCase))
                {
                    findings.Add(new { Severity = "High", Description = "Cosmos DB account allows public access", Recommendation = "Consider disabling public access and use private endpoints" });
                }

                // Add more checks here as needed

                var scanResult = new
                {
                    ResourceId = resourceId,
                    ResourceName = accountInfo.Value.Data.Name,
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
    }
}