using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Security.Claims;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.CosmosDB;
using Azure.Core;
using Microsoft.Azure.Cosmos;

namespace SecurityScanFunction
{
    public class SecurityScanTrigger
    {
        private readonly CosmosDBSecurityScanner _scanner;
        private readonly ScanResultStore _store;

        public SecurityScanTrigger(CosmosDBSecurityScanner scanner, ScanResultStore store)
        {
            _scanner = scanner;
            _store = store;
        }

        [FunctionName("SecurityScanTrigger")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log,
            ClaimsPrincipal claimsPrincipal)
        {
            if (!claimsPrincipal.Identity.IsAuthenticated)
            {
                return new UnauthorizedResult();
            }

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
                var cosmosDBAccountResource = armClient.GetCosmosDBAccountResource(new ResourceIdentifier(resourceId));

                // Get the Cosmos DB account data
                var accountResponse = await cosmosDBAccountResource.GetAsync();
                if (!accountResponse.HasValue)
                {
                    return new NotFoundResult();
                }

                var scanResult = await _scanner.ScanCosmosDBAccount(accountResponse.Value, log);
                await _store.StoreScanResult(scanResult);

                var severityCounts = scanResult.Findings
                    .GroupBy(f => f.Severity)
                    .ToDictionary(g => g.Key, g => g.Count());

                var response = new
                {
                    scanResult.Id,
                    scanResult.ResourceId,
                    scanResult.ResourceName,
                    scanResult.ScanTime,
                    SeverityCounts = severityCounts,
                    scanResult.Findings
                };

                log.LogInformation($"Scan completed successfully for resourceId: {resourceId}");
                return new OkObjectResult(response);
            }
            catch (Azure.Identity.AuthenticationFailedException authEx)
            {
                log.LogError(authEx, "Authentication failed");
                return new UnauthorizedResult();
            }
            catch (Azure.RequestFailedException reqEx)
            {
                log.LogError(reqEx, "Azure request failed");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            catch (CosmosException cosmosEx)
            {
                log.LogError(cosmosEx, "Cosmos DB operation failed");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An unexpected error occurred.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        public class GetScanResults
        {
            [FunctionName("GetScanResults")]
            public async Task<IActionResult> Run(
                [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
                ILogger log,
                ClaimsPrincipal claimsPrincipal)
            {
                if (!claimsPrincipal.Identity.IsAuthenticated)
                {
                    return new UnauthorizedResult();
                }
                // Logic to fetch and return scan results...
            }
        }
    }
}