using SecurityScanFunction.Tests.Interfaces;
using SecurityScanFunction.Tests.Models;

namespace SecurityScanFunction.Tests.Services
{
    public class TestCosmosDBSecurityScanner : ICosmosDBSecurityScanner
    {
        public async Task<ScanResult> ScanAsync(string accountId)
        {
            var result = new ScanResult
            {
                AccountId = accountId,
                ScanTime = DateTime.UtcNow,
                Issues = new List<SecurityIssue>()
            };

            // Add sample security issues
            result.Issues.Add(new SecurityIssue
            {
                Category = "NetworkSecurity",
                Severity = IssueSeverity.High,
                Description = "Public network access is enabled",
                Recommendation = "Disable public network access and use private endpoints"
            });

            return result;
        }
    }
}