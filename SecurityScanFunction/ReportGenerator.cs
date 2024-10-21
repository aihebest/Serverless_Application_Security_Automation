using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace SecurityScanFunction
{
    public class ReportGenerator
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;

        public ReportGenerator(string connectionString)
        {
            _cosmosClient = new CosmosClient(connectionString);
            _container = _cosmosClient.GetContainer("SecurityScans", "ScanResults");
        }

        public async Task<string> GenerateReport(DateTime startDate, DateTime endDate, ILogger log)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.ScanTime >= @startDate AND c.ScanTime <= @endDate")
                .WithParameter("@startDate", startDate)
                .WithParameter("@endDate", endDate);

            var iterator = _container.GetItemQueryIterator<ScanResult>(query);
            var results = new List<ScanResult>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            var report = new StringBuilder();
            report.AppendLine($"Security Scan Report ({startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd})");
            report.AppendLine("================================================");
            report.AppendLine($"Total scans: {results.Count}");
            report.AppendLine();

            var severityCounts = results.SelectMany(r => r.Findings)
                .GroupBy(f => f.Severity)
                .OrderByDescending(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var severity in severityCounts)
            {
                report.AppendLine($"{severity.Key} severity findings: {severity.Value}");
            }

            report.AppendLine();
            report.AppendLine("Top 5 most common findings:");
            var topFindings = results.SelectMany(r => r.Findings)
                .GroupBy(f => f.Description)
                .OrderByDescending(g => g.Count())
                .Take(5);

            foreach (var finding in topFindings)
            {
                report.AppendLine($"- {finding.Key}: {finding.Count()} occurrences");
            }

            return report.ToString();
        }
    }
}