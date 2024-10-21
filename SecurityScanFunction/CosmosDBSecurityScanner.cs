using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Azure.ResourceManager.CosmosDB;

namespace SecurityScanFunction
{
    public class CosmosDBSecurityScanner
    {
        public async Task<ScanResult> ScanCosmosDBAccount(CosmosDBAccountResource accountResource, ILogger log)
        {
            await Task.Yield();

            var findings = new List<Finding>();
            var account = accountResource.Data;

            // Check for public network access
            var publicNetworkAccess = GetPropertyValue(account, "PublicNetworkAccess");
            if (publicNetworkAccess?.ToString() != "Disabled")
            {
                findings.Add(new Finding
                {
                    Severity = "High",
                    Description = "Public network access is enabled",
                    Recommendation = "Disable public network access and use private endpoints"
                });
            }

            // Check for IP firewall rules
            var ipRules = GetPropertyValue(account, "IpRules") as IEnumerable<object>;
            if (ipRules == null || !ipRules.GetEnumerator().MoveNext())
            {
                findings.Add(new Finding
                {
                    Severity = "Medium",
                    Description = "No IP firewall rules configured",
                    Recommendation = "Configure IP firewall rules to restrict access"
                });
            }

            // Check for automatic failover
            var enableAutomaticFailover = GetPropertyValue(account, "EnableAutomaticFailover");
            if (enableAutomaticFailover == null || !(bool)enableAutomaticFailover)
            {
                findings.Add(new Finding
                {
                    Severity = "Low",
                    Description = "Automatic failover is not enabled",
                    Recommendation = "Enable automatic failover for improved availability"
                });
            }

            // Check for VNET integration
            var isVirtualNetworkFilterEnabled = GetPropertyValue(account, "IsVirtualNetworkFilterEnabled");
            if (isVirtualNetworkFilterEnabled == null || !(bool)isVirtualNetworkFilterEnabled)
            {
                findings.Add(new Finding
                {
                    Severity = "Medium",
                    Description = "VNET integration is not enabled",
                    Recommendation = "Enable VNET integration for improved network security"
                });
            }

            // Check for encryption at rest
            var keyVaultKeyUri = GetPropertyValue(account, "KeyVaultKeyUri");
            if (string.IsNullOrEmpty(keyVaultKeyUri?.ToString()))
            {
                findings.Add(new Finding
                {
                    Severity = "High",
                    Description = "Customer-managed key is not used for encryption at rest",
                    Recommendation = "Configure a customer-managed key in Azure Key Vault for encryption at rest"
                });
            }

            // Check for multi-region writes
            var enableMultipleWriteLocations = GetPropertyValue(account, "EnableMultipleWriteLocations");
            if (enableMultipleWriteLocations == null || !(bool)enableMultipleWriteLocations)
            {
                findings.Add(new Finding
                {
                    Severity = "Low",
                    Description = "Multi-region writes are not enabled",
                    Recommendation = "Enable multi-region writes for improved availability and performance"
                });
            }

            return new ScanResult
            {
                Id = Guid.NewGuid().ToString(),
                ResourceId = accountResource.Id,
                ResourceName = account.Name,
                ScanTime = DateTime.UtcNow,
                Findings = findings
            };
        }

        private static object GetPropertyValue(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName)?.GetValue(obj, null);
        }
    }
}