using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Azure.Cosmos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using SecurityScanFunction.Services;

[assembly: FunctionsStartup(typeof(SecurityScanFunction.Startup))]

namespace SecurityScanFunction
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Add Cosmos DB client
            builder.Services.AddSingleton(s => 
            {
                var connectionString = Environment.GetEnvironmentVariable("CosmosDBConnectionString");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("CosmosDBConnectionString is required");
                }
                return new CosmosClient(connectionString);
            });

            // Add logging
            builder.Services.AddLogging();

            // Add security scanner
            builder.Services.AddSingleton<SecurityScanner>();
            builder.Services.AddSingleton<ScanResultStore>();

            // Add authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = $"https://login.microsoftonline.com/{Environment.GetEnvironmentVariable("TENANT_ID")}/v2.0";
                options.Audience = Environment.GetEnvironmentVariable("AUTH_AUDIENCE");
            });

            // Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });
        }
    }
}