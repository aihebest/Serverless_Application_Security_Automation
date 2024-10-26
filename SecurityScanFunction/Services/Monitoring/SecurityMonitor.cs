using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights;
using System.Linq;

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
                // Track overall scan metrics
                _telemetryClient.TrackEvent("SecurityScanCompleted", 
                    new Dictionary<string, string>
                    {
                        { "ResourceId", scanResult.ResourceId },
                        { "ResourceName", scanResult.ResourceName },
                        { "ScanTime", scanResult.ScanTime.ToString() }
                    });

                // Track severity metrics
                var severityMetrics = scanResult.Findings
                    .GroupBy(f => f.Severity)
                    .ToDictionary(g => g.Key, g => g.Count());

                _telemetryClient.TrackMetric("HighSeverityFindings", 
                    severityMetrics.GetValueOrDefault("High", 0));
                _telemetryClient.TrackMetric("MediumSeverityFindings", 
                    severityMetrics.GetValueOrDefault("Medium", 0));
                _telemetryClient.TrackMetric("LowSeverityFindings", 
                    severityMetrics.GetValueOrDefault("Low", 0));

                // Track critical findings separately
                var criticalFindings = scanResult.Findings
                    .Where(f => f.Severity == "High")
                    .ToList();

                foreach (var finding in criticalFindings)
                {
                    _telemetryClient.TrackEvent("CriticalSecurityFinding",
                        new Dictionary<string, string>
                        {
                            { "ResourceId", scanResult.ResourceId },
                            { "Description", finding.Description },
                            { "Recommendation", finding.Recommendation }
                        });

                    // Log critical findings
                    _logger.LogWarning(
                        $"Critical security finding for {scanResult.ResourceName}: {finding.Description}");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking security scan");
                throw;
            }
        }

        public async Task TrackSecurityAlert(SecurityAlert alert)
        {
            try
            {
                _telemetryClient.TrackEvent("SecurityAlert",
                    new Dictionary<string, string>
                    {
                        { "ResourceId", alert.ResourceId },
                        { "AlertType", alert.AlertType },
                        { "Severity", alert.Severity },
                        { "Description", alert.Description }
                    });

                if (alert.Severity == "High")
                {
                    _logger.LogError($"High severity security alert: {alert.Description}");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking security alert");
                throw;
            }
        }
    }

    public class SecurityAlert
    {
        public string ResourceId { get; set; }
        public string AlertType { get; set; }
        public string Severity { get; set; }
        public string Description { get; set; }
        public DateTime AlertTime { get; set; }
    }
}