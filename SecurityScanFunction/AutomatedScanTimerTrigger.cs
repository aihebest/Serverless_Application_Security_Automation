using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.CosmosDB;
using Azure.Core;
using Microsoft.Azure.Cosmos;

namespace SecurityScanFunction
{
    public class AutomatedScanTimerTrigger
    {
        private readonly CosmosDBSecurityScanner _scanner;
        private readonly ScanResultStore _store;

        public AutomatedScanTimerTrigger(CosmosDBSecurityScanner scanner, ScanResultStore store)
        {
            _scanner = scanner;
            _store = store;
        }

        [FunctionName("AutomatedScanTimerTrigger")]
        public async Task Run([TimerTrigger("0 0 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Automated scan timer trigger function executed at: {DateTime.Now}");

            try
            {
                var credential = new DefaultAzureCredential();
                var armClient = new ArmClient(credential);
                
                var subscription = await armClient.GetDefaultSubscriptionAsync();
                var resourceGroups = subscription.GetResourceGroups();

                await foreach (var resourceGroup in resourceGroups.GetAllAsync())
                {
                    var cosmosDBAccounts = resourceGroup.GetCosmosDBAccounts();
                    await foreach (var account in cosmosDBAccounts.GetAllAsync())
                    {
                        var scanResult = await _scanner.ScanCosmosDBAccount(account, log);
                        await _store.StoreScanResult(scanResult);
                        log.LogInformation($"Scan completed and stored for account: {account.Data.Name}");
                    }
                }
            }
             catch (Exception ex)
            {
                log.LogError(ex, "Error occurred during automated scan");
            }
        }
    }
}