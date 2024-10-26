using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SecurityScanFunction.Tests.Interfaces;

namespace SecurityScanFunction.Tests
{
    public abstract class TestBase
    {
        protected IServiceCollection Services;
        protected IConfiguration Configuration;

        protected TestBase()
        {
            // Setup configuration
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = configBuilder.Build();

            // Setup services
            Services = new ServiceCollection();
            Services.AddLogging();
            Services.AddSingleton(Configuration);
        }
    }
}