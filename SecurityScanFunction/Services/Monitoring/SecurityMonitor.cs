using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;

namespace SecurityScanFunction.Services.Monitoring
{
    public class SecurityMonitor
    {
        private readonly ILogger<SecurityMonitor> _logger;
        private readonly TelemetryClient _telemetryClient;

        public SecurityMonitor(
            ILogger<SecurityMonitor> logger,
            TelemetryClient telemetryClient)
        {
            _logger = logger;
            _telemetryClient = telemetryClient;
        }

        public async Task TrackSecurityScan(ScanResult scanResult)
        {
            try
            {
                _telemetryClient.TrackEvent("SecurityScanCompleted", new Dictionary<string, string>
                {
                    { "ResourceId", scanResult.ResourceId },
                    { "ResourceName", scanResult.ResourceName },
                    { "ScanTime", scanResult.ScanTime.ToString() }
                });

                _logger.LogInformation($"Tracked scan results for resource {scanResult.ResourceName}");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking security scan");
                throw;
            }
        }
    }
}