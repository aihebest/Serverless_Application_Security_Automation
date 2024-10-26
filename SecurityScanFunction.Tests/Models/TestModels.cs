using System;
using System.Collections.Generic;

namespace SecurityScanFunction.Tests.Models
{
    // Existing model classes...
    public class CosmosDBAccount
    {
        public string Id { get; set; }
        public CosmosDBAccountProperties Properties { get; set; }
    }

    public class CosmosDBAccountProperties
    {
        public string PublicNetworkAccess { get; set; }
        public bool EnableFreeTier { get; set; }
        public bool EnableAutomaticFailover { get; set; }
        public bool EnableMultipleWriteLocations { get; set; }
        public List<IpRule> IpRules { get; set; }
        public string NetworkAclBypass { get; set; }
        public bool DisableKeyBasedMetadataWriteAccess { get; set; }
        public bool EnableAnalyticalStorage { get; set; }
    }

    public class IpRule
    {
        public string IpAddressOrRange { get; set; }
    }

    public class SecurityIssue
    {
        public string Category { get; set; }
        public IssueSeverity Severity { get; set; }
        public string Description { get; set; }
        public string Recommendation { get; set; }
    }

    public enum IssueSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }
}