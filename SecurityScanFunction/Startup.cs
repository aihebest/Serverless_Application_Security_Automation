using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using SecurityScanFunction.Services;
using SecurityScanFunction.Interfaces;
using Microsoft.Azure.Cosmos;

[assembly: FunctionsStartup(typeof(SecurityScanFunction.Startup))]

namespace SecurityScanFunction
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Add CosmosClient
            builder.Services.AddSingleton(s => 
            {
                var connectionString = Environment.GetEnvironmentVariable("CosmosDBConnectionString");
                return new CosmosClient(connectionString);
            });

            // Add Security Services
            builder.Services.AddScoped<ICosmosOperations, SecurityScanner>();
            builder.Services.AddScoped<SecurityScanner>();
            
            // Add Storage Services
            builder.Services.AddSingleton(s => 
            {
                var connectionString = Environment.GetEnvironmentVariable("CosmosDBConnectionString");
                return new ScanResultStore(connectionString);
            });
            
            // Add Report Services
            builder.Services.AddSingleton(s => 
            {
                var connectionString = Environment.GetEnvironmentVariable("CosmosDBConnectionString");
                return new ReportGenerator(connectionString);
            });

            // Add Logger Configuration
            builder.Services.AddLogging();

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

            // Add Configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            builder.Services.AddSingleton<IConfiguration>(config);
        }

        private static CosmosClient InitializeCosmosClientInstance(string connectionString)
        {
            var clientOptions = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            };

            return new CosmosClient(connectionString, clientOptions);
        }
    }
}