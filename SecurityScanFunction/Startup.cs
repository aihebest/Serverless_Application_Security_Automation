using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using SecurityScanFunction;

[assembly: FunctionsStartup(typeof(SecurityScanFunction.Startup))]

namespace SecurityScanFunction
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<CosmosDBSecurityScanner>();
            builder.Services.AddSingleton(s => new ScanResultStore(Environment.GetEnvironmentVariable("CosmosDBConnectionString")));
            builder.Services.AddSingleton(s => new ReportGenerator(Environment.GetEnvironmentVariable("CosmosDBConnectionString")));
        }
    }
}