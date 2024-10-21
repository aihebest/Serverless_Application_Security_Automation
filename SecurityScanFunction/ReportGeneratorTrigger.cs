using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SecurityScanFunction
{
    public class ReportGeneratorTrigger
    {
        private readonly ReportGenerator _reportGenerator;

        public ReportGeneratorTrigger(ReportGenerator reportGenerator)
        {
            _reportGenerator = reportGenerator;
        }

        [FunctionName("ReportGeneratorTrigger")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a report generation request.");

            string startDateString = req.Query["startDate"];
            string endDateString = req.Query["endDate"];

            if (!DateTime.TryParse(startDateString, out DateTime startDate) || !DateTime.TryParse(endDateString, out DateTime endDate))
            {
                return new BadRequestObjectResult("Please provide valid startDate and endDate query parameters.");
            }

            try
            {
                string report = await _reportGenerator.GenerateReport(startDate, endDate, log);
                return new OkObjectResult(report);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while generating the report.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}