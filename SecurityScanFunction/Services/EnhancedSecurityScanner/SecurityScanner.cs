using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using SecurityScanFunction.Interfaces;

namespace SecurityScanFunction.Services
{
    public class SecurityScanner : ICosmosOperations
    {
        private readonly ILogger<SecurityScanner> _logger;
        private readonly CosmosClient _cosmosClient;
        private readonly ScanResultStore _scanResultStore;

        public SecurityScanner(
            ILogger<SecurityScanner> logger,
            CosmosClient cosmosClient,
            ScanResultStore scanResultStore)
        {
            _logger = logger;
            _cosmosClient = cosmosClient;
            _scanResultStore = scanResultStore;
        }

        // Explicitly implement the interface method
        async Task<CosmosAccountSettings> ICosmosOperations.GetAccountSettingsAsync()
        {
            try
            {
                _logger.LogInformation("Getting Cosmos DB account settings");
                
                // For development, return mock settings
                return new CosmosAccountSettings
                {
                    PublicNetworkAccess = true,
                    IpRules = new[] { "0.0.0.0/0" },
                    KeyVaultKeyUri = null,
                    EnableAutomaticFailover = false,
                    EnableMultipleWriteLocations = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Cosmos DB account settings");
                throw;
            }
        }

        private async Task<CosmosAccountSettings> GetAccountSettingsInternalAsync()
        {
            return await ((ICosmosOperations)this).GetAccountSettingsAsync();
        }

        public async Task<ScanResult> ScanResource(string resourceId)
        {
            _logger.LogInformation($"Starting security scan for resource {resourceId}");

            try
            {
                var scanResult = new ScanResult
                {
                    Id = Guid.NewGuid().ToString(),
                    ResourceId = resourceId,
                    ScanTime = DateTime.UtcNow,
                    Findings = new List<Finding>()
                };

                var accountSettings = await GetAccountSettingsInternalAsync();
                await PerformSecurityChecks(scanResult, accountSettings);
                
                // Store the scan result
                await _scanResultStore.StoreScanResult(scanResult);
                _logger.LogInformation($"Scan completed and stored for resource {resourceId}");
                
                return scanResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error scanning resource {resourceId}");
                throw;
            }
        }

        private async Task PerformSecurityChecks(ScanResult result, CosmosAccountSettings settings)
        {
            // Keep your existing security checks implementation
            result.Findings.Add(new Finding
            {
                Severity = settings.PublicNetworkAccess ? "High" : "Low",
                Description = "Public Network Access Check",
                Recommendation = settings.PublicNetworkAccess ? 
                    "Disable public network access and use private endpoints" : 
                    "Public network access is properly restricted"
            });

            // Keep the rest of your existing checks...
            
            await Task.CompletedTask;
        }
    }
}