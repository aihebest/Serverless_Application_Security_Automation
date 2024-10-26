using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SecurityScanFunction.Interfaces;

namespace SecurityScanFunction
{
    public class CosmosDBSecurityScanner
    {
        private readonly ILogger<CosmosDBSecurityScanner> _logger;
        private readonly ICosmosOperations _cosmosOperations;

        public CosmosDBSecurityScanner(
            ILogger<CosmosDBSecurityScanner> logger,
            ICosmosOperations cosmosOperations)
        {
            _logger = logger;
            _cosmosOperations = cosmosOperations;
        }

        public async Task<ScanResult> ScanResource(string resourceId)
        {
            _logger.LogInformation($"Starting Cosmos DB security scan for resource {resourceId}");
            
            try
            {
                var scanResult = new ScanResult
                {
                    Id = Guid.NewGuid().ToString(),
                    ResourceId = resourceId,
                    ResourceName = "Cosmos DB Account",
                    ScanTime = DateTime.UtcNow,
                    Findings = new List<Finding>()
                };

                var accountSettings = await _cosmosOperations.GetAccountSettingsAsync();
                
                // Network Security
                scanResult.Findings.Add(new Finding
                {
                    Severity = accountSettings.PublicNetworkAccess ? "High" : "Low",
                    Description = "Public Network Access Check",
                    Recommendation = accountSettings.PublicNetworkAccess ? 
                        "Disable public network access and use private endpoints" : 
                        "Public network access is properly restricted"
                });

                // Data Security
                scanResult.Findings.Add(new Finding
                {
                    Severity = string.IsNullOrEmpty(accountSettings.KeyVaultKeyUri) ? "High" : "Low",
                    Description = "Encryption at Rest Configuration",
                    Recommendation = "Enable customer-managed keys for better security"
                });

                // Availability
                scanResult.Findings.Add(new Finding
                {
                    Severity = "Medium",
                    Description = "Automatic Failover Configuration",
                    Recommendation = accountSettings.EnableAutomaticFailover ? 
                        "Automatic failover is properly configured" : 
                        "Enable automatic failover for better availability"
                });

                return scanResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error scanning Cosmos DB resource {resourceId}");
                throw;
            }
        }
    }
}