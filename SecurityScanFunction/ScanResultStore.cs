using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace SecurityScanFunction
{
    public class ScanResultStore
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;

        public ScanResultStore(string connectionString)
        {
            _cosmosClient = new CosmosClient(connectionString);
            _container = _cosmosClient.GetContainer("SecurityScans", "ScanResults");
        }

        public async Task StoreScanResult(ScanResult scanResult)
        {
            try
            {
                Console.WriteLine($"Attempting to store scan result: Id={scanResult.Id}, ResourceId={scanResult.ResourceId}");
                if (string.IsNullOrEmpty(scanResult.Id))
                {
                    scanResult.Id = Guid.NewGuid().ToString();
                    Console.WriteLine($"Generated new Id: {scanResult.Id}");
                }
                var response = await _container.CreateItemAsync(scanResult, new PartitionKey(scanResult.Id));
                Console.WriteLine($"Stored scan result successfully. Status code: {response.StatusCode}");
            }
            catch (CosmosException ce)
            {
                Console.WriteLine($"Cosmos DB Error: {ce.StatusCode}, {ce.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error storing scan result: {ex.Message}");
                throw;
            }
        }
    }
}