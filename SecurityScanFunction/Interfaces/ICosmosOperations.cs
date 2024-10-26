using System.Threading.Tasks;

namespace SecurityScanFunction.Interfaces
{
    public interface ICosmosOperations
    {
        Task<CosmosAccountSettings> GetAccountSettingsAsync();
    }

    public class CosmosAccountSettings
    {
        public bool PublicNetworkAccess { get; set; }
        public string[] IpRules { get; set; }
        public string KeyVaultKeyUri { get; set; }
        public bool EnableAutomaticFailover { get; set; }
        public bool EnableMultipleWriteLocations { get; set; }
    }
}