using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace SecurityScanFunction
{
    public class CosmosDbService
    {
        private Container _container;

        public CosmosDbService(IConfiguration configuration)
        {
            string connectionString = configuration["CosmosDbConnectionString"];
            string databaseName = configuration["CosmosDbDatabaseName"];
            string containerName = configuration["CosmosDbContainerName"];

            CosmosClient client = new CosmosClient(connectionString);
            _container = client.GetContainer(databaseName, containerName);
        }

        public async Task<ScanResult> AddScanResultAsync(ScanResult scanResult)
        {
            scanResult.Id = Guid.NewGuid().ToString();
            scanResult.ScanTime = DateTime.UtcNow;
            return await _container.CreateItemAsync(scanResult, new PartitionKey(scanResult.ResourceId));
        }

        public async Task<ScanResult> GetScanResultAsync(string id, string resourceId)
        {
            try
            {
                return await _container.ReadItemAsync<ScanResult>(id, new PartitionKey(resourceId));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }
    }
}