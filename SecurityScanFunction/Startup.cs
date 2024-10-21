using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using SecurityScanFunction;
using Microsoft.Azure.Cosmos;

[assembly: FunctionsStartup(typeof(SecurityScanFunction.Startup))]

namespace SecurityScanFunction
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<CosmosDBSecurityScanner>();
            
            builder.Services.AddSingleton(s => 
            {
                var connectionString = Environment.GetEnvironmentVariable("CosmosDBConnectionString");
                return new ScanResultStore(connectionString);
            });
            
            builder.Services.AddSingleton(s => 
            {
                var connectionString = Environment.GetEnvironmentVariable("CosmosDBConnectionString");
                return new ReportGenerator(connectionString);
            });

            // Add CORS policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowStaticWebApp",
                    builder => builder.WithOrigins(Environment.GetEnvironmentVariable("CORS"))
                                      .AllowAnyMethod()
                                      .AllowAnyHeader());
            });

            // Add authentication
            builder.Services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Audience = Environment.GetEnvironmentVariable("AUTH_AUDIENCE");
                options.Authority = $"https://login.microsoftonline.com/{Environment.GetEnvironmentVariable("TENANT_ID")}/v2.0";
            });
        }
    }
}