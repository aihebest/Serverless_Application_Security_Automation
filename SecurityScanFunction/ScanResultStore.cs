using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace SecurityScanFunction
{
    public class ScanResultStore
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;
        private readonly ILogger<ScanResultStore> _logger;

        public ScanResultStore(string connectionString, ILogger<ScanResultStore> logger = null)
        {
            _cosmosClient = new CosmosClient(connectionString);
            _container = _cosmosClient.GetContainer("SecurityScans", "ScanResults");
            _logger = logger;
        }

        public async Task StoreScanResult(ScanResult scanResult)
        {
            try
            {
                _logger?.LogInformation($"Attempting to store scan result: Id={scanResult.Id}, ResourceId={scanResult.ResourceId}");
                
                if (string.IsNullOrEmpty(scanResult.Id))
                {
                    scanResult.Id = Guid.NewGuid().ToString();
                    _logger?.LogInformation($"Generated new Id: {scanResult.Id}");
                }

                var response = await _container.CreateItemAsync(scanResult, 
                    new PartitionKey(scanResult.ResourceId));
                
                _logger?.LogInformation($"Stored scan result successfully. Status code: {response.StatusCode}");
            }
            catch (CosmosException ce)
            {
                _logger?.LogError($"Cosmos DB Error: {ce.StatusCode}, {ce.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error storing scan result: {ex.Message}");
                throw;
            }
        }

        public async Task<ScanResult> GetLatestScanForResource(string resourceId)
        {
            try
            {
                var query = new QueryDefinition(
                    "SELECT TOP 1 * FROM c WHERE c.resourceId = @resourceId ORDER BY c.scanTime DESC")
                    .WithParameter("@resourceId", resourceId);

                var iterator = _container.GetItemQueryIterator<ScanResult>(query);
                var results = await iterator.ReadNextAsync();
                
                return results.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error retrieving latest scan for resource {resourceId}: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<ScanResult>> GetScanHistory(string resourceId, int limit = 10)
        {
            try
            {
                var query = new QueryDefinition(
                    "SELECT TOP @limit * FROM c WHERE c.resourceId = @resourceId ORDER BY c.scanTime DESC")
                    .WithParameter("@limit", limit)
                    .WithParameter("@resourceId", resourceId);

                var results = new List<ScanResult>();
                var iterator = _container.GetItemQueryIterator<ScanResult>(query);

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    results.AddRange(response);
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error retrieving scan history for resource {resourceId}: {ex.Message}");
                throw;
            }
        }

        public async Task<IDictionary<string, int>> GetSecurityMetrics(string resourceId)
        {
            try
            {
                var latestScan = await GetLatestScanForResource(resourceId);
                if (latestScan == null)
                    return new Dictionary<string, int>();

                return latestScan.Findings
                    .GroupBy(f => f.Severity)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error calculating security metrics for resource {resourceId}: {ex.Message}");
                throw;
            }
        }
    }
}