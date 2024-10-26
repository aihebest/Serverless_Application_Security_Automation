using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace SecurityScanFunction
{
    public class AutomatedScanTimerTrigger
    {
        private readonly CosmosDBSecurityScanner _securityScanner;
        private readonly ILogger<AutomatedScanTimerTrigger> _logger;
        private readonly IConfiguration _configuration;

        public AutomatedScanTimerTrigger(
            CosmosDBSecurityScanner securityScanner,
            ILogger<AutomatedScanTimerTrigger> logger,
            IConfiguration configuration)
        {
            _securityScanner = securityScanner;
            _logger = logger;
            _configuration = configuration;
        }

        [FunctionName("AutomatedScanTimerTrigger")]
        public async Task Run([TimerTrigger("0 0 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"Security scan timer trigger executed at: {DateTime.UtcNow}");

            try
            {
                var resourceIds = _configuration.GetSection("MonitoredResources").Get<string[]>();
                
                foreach (var resourceId in resourceIds)
                {
                    _logger.LogInformation($"Starting scan for resource: {resourceId}");
                    
                    var scanResult = await _securityScanner.ScanResource(resourceId);
                    
                    // Log findings summary
                    var criticalFindings = scanResult.Findings.FindAll(f => f.Severity == "High");
                    if (criticalFindings.Count > 0)
                    {
                        _logger.LogWarning($"Found {criticalFindings.Count} critical security issues in {resourceId}");
                    }

                    // Store results (implement as needed)
                    await StoreScanResults(scanResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during automated security scan");
                throw;
            }
        }

        private async Task StoreScanResults(ScanResult scanResult)
        {
            // Implement storing results in Cosmos DB
            await Task.CompletedTask;
        }
    }
}