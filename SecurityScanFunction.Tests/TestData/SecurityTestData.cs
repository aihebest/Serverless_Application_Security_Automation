using System.Collections.Generic;
using SecurityScanFunction.Models;
using SecurityScanFunction.Tests.Models;

namespace SecurityScanFunction.Tests.TestData
{
    public static class SecurityTestData
    {
        public static CosmosDBAccount GetTestAccount()
        {
            return new CosmosDBAccount
            {
                Id = "test-account",
                Properties = new CosmosDBAccountProperties
                {
                    PublicNetworkAccess = "Enabled",
                    EnableFreeTier = false,
                    EnableAutomaticFailover = false,
                    EnableMultipleWriteLocations = false,
                    IpRules = new List<IpRule>
                    {
                        new IpRule { IpAddressOrRange = "0.0.0.0/0" }
                    },
                    NetworkAclBypass = "None",
                    DisableKeyBasedMetadataWriteAccess = false,
                    EnableAnalyticalStorage = false
                }
            };
        }

        public static List<SecurityIssue> GetExpectedSecurityIssues()
        {
            return new List<SecurityIssue>
            {
                new SecurityIssue
                {
                    Category = "NetworkSecurity",
                    Severity = IssueSeverity.High,
                    Description = "Public network access is enabled",
                    Recommendation = "Disable public network access and use private endpoints"
                },
                new SecurityIssue
                {
                    Category = "NetworkSecurity",
                    Severity = IssueSeverity.Medium,
                    Description = "IP range 0.0.0.0/0 allows access from any IP",
                    Recommendation = "Restrict IP ranges to only necessary addresses"
                },
                new SecurityIssue
                {
                    Category = "Availability",
                    Severity = IssueSeverity.Medium,
                    Description = "Automatic failover is not enabled",
                    Recommendation = "Enable automatic failover for better availability"
                }
            };
        }
    }
}