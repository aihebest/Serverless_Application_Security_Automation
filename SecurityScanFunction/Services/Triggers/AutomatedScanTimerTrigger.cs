using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SecurityScanFunction.Services.SecurityChecks;

namespace SecurityScanFunction.Services.Triggers
{
    public class AutomatedScanTimerTrigger
    {
        private readonly SecurityCheckService _securityCheckService;
        private readonly ILogger<AutomatedScanTimerTrigger> _logger;

        public AutomatedScanTimerTrigger(
            SecurityCheckService securityCheckService,
            ILogger<AutomatedScanTimerTrigger> logger)
        {
            _securityCheckService = securityCheckService;
            _logger = logger;
        }

        [FunctionName("AutomatedScanTimer")]
        public async Task Run([TimerTrigger("0 0 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"Security scan timer trigger executed at: {DateTime.Now}");

            try
            {
                var resourceId = Environment.GetEnvironmentVariable("DefaultResourceId");
                var resourceName = Environment.GetEnvironmentVariable("DefaultResourceName");

                var result = await _securityCheckService.PerformSecurityCheck(resourceId, resourceName);
                _logger.LogInformation($"Completed automated security scan for resource {resourceName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during automated security scan");
                throw;
            }
        }
    }
}