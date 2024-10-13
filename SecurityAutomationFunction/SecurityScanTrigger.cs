using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos;

public static class SecurityScanTrigger
{
    private static readonly string ConnectionString = Environment.GetEnvironmentVariable("CosmosDBConnection");
    private static readonly CosmosClient CosmosClient = new CosmosClient(ConnectionString);
    private static readonly Container Container = CosmosClient.GetContainer("SecurityScans", "ScanResults");

    [FunctionName("SecurityScanTrigger")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation($"Security scan triggered at: {DateTime.UtcNow}");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);

        string resourceToScan = data?.resourceToScan;
        string scanType = data?.scanType;

        if (string.IsNullOrEmpty(resourceToScan) || string.IsNullOrEmpty(scanType))
        {
            log.LogWarning("Invalid request: Missing required parameters");
            return new BadRequestObjectResult("Please provide resourceToScan and scanType in the request body");
        }

        log.LogInformation($"Initiating {scanType} scan for resource: {resourceToScan}");

        try
        {
            var scanResults = await PerformSecurityScan(resourceToScan, scanType);
            
            // Store results in Cosmos DB using SDK
            try
            {
                var response = await Container.CreateItemAsync(new
                {
                    id = Guid.NewGuid().ToString(),
                    timestamp = DateTime.UtcNow,
                    resourceScanned = resourceToScan,
                    scanType = scanType,
                    results = scanResults
                });
                log.LogInformation($"Item created in Cosmos DB. Request charge: {response.RequestCharge}");
            }
            catch (CosmosException ex)
            {
                log.LogError($"Cosmos DB Error: {ex.StatusCode}, {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return new OkObjectResult(scanResults);
        }
        catch (Exception ex)
        {
            log.LogError($"An error occurred during the security scan for {resourceToScan}: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<object> PerformSecurityScan(string resourceId, string scanType)
    {
        // Simulate an asynchronous operation
        await Task.Delay(1000);

        var vulnerabilities = new List<object>();
        var recommendations = new List<string>();

        switch (scanType.ToLower())
        {
            case "vulnerabilityassessment":
                SimulateVulnerabilityAssessment(resourceId, vulnerabilities, recommendations);
                break;
            case "compliancecheck":
                SimulateComplianceCheck(resourceId, vulnerabilities, recommendations);
                break;
            case "configurationaudit":
                SimulateConfigurationAudit(resourceId, vulnerabilities, recommendations);
                break;
            default:
                throw new ArgumentException($"Invalid scan type: {scanType}");
        }

        return new
        {
            Timestamp = DateTime.UtcNow,
            ScannedResource = resourceId,
            ScanType = scanType,
            Vulnerabilities = vulnerabilities,
            Recommendations = recommendations
        };
    }

    private static void SimulateVulnerabilityAssessment(string resourceId, List<object> vulnerabilities, List<string> recommendations)
    {
        if (resourceId == "secure-webapp-002")
        {
            // No vulnerabilities for this specific resource
            recommendations.Add("No vulnerabilities found. Continue maintaining security best practices.");
            return;
        }

        vulnerabilities.Add(new { Description = "Potential SQL Injection vulnerability", Severity = "High", ResourceId = resourceId });
        vulnerabilities.Add(new { Description = "Outdated SSL/TLS version", Severity = "Medium", ResourceId = resourceId });
        recommendations.Add("Implement input validation and use parameterized queries");
        recommendations.Add("Upgrade to the latest SSL/TLS version");
    }

    private static void SimulateComplianceCheck(string resourceId, List<object> vulnerabilities, List<string> recommendations)
    {
        vulnerabilities.Add(new { Description = "Non-compliance with data retention policy", Severity = "Medium", ResourceId = resourceId });
        vulnerabilities.Add(new { Description = "Lack of encryption for data at rest", Severity = "High", ResourceId = resourceId });
        recommendations.Add("Implement data lifecycle management policy");
        recommendations.Add("Enable encryption for all data storage solutions");
    }

    private static void SimulateConfigurationAudit(string resourceId, List<object> vulnerabilities, List<string> recommendations)
    {
        vulnerabilities.Add(new { Description = "Excessive permissions in IAM roles", Severity = "High", ResourceId = resourceId });
        vulnerabilities.Add(new { Description = "Default network security group rules", Severity = "Medium", ResourceId = resourceId });
        recommendations.Add("Review and adjust IAM roles to follow least privilege principle");
        recommendations.Add("Customize network security group rules based on application requirements");
    }
}