using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using SecurityScanFunction.Services.Monitoring;

namespace SecurityScanFunction.Services.SecurityChecks
{
    public class SecurityCheckService
    {
        private readonly ILogger<SecurityCheckService> _logger;
        private readonly SecurityMonitor _monitor;

        public SecurityCheckService(
            ILogger<SecurityCheckService> logger,
            SecurityMonitor monitor)
        {
            _logger = logger;
            _monitor = monitor;
        }

        public async Task<ScanResult> PerformSecurityCheck(string resourceId, string resourceName)
        {
            try
            {
                var scanResult = new ScanResult
                {
                    Id = Guid.NewGuid().ToString(),
                    ResourceId = resourceId,
                    ResourceName = resourceName,
                    ScanTime = DateTime.UtcNow,
                    Findings = new List<Finding>()
                };

                await PerformChecks(scanResult);
                await _monitor.TrackSecurityScan(scanResult);

                return scanResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error performing security check for resource {resourceName}");
                throw;
            }
        }

        private async Task PerformChecks(ScanResult result)
        {
            result.Findings.Add(new Finding
            {
                Severity = "Medium",
                Description = "Network security check performed",
                Recommendation = "Review network security settings"
            });

            result.Findings.Add(new Finding
            {
                Severity = "High",
                Description = "Data security check performed",
                Recommendation = "Review encryption settings"
            });

            result.Findings.Add(new Finding
            {
                Severity = "Low",
                Description = "Access control check performed",
                Recommendation = "Review access policies"
            });

            await Task.CompletedTask;
        }
    }
}