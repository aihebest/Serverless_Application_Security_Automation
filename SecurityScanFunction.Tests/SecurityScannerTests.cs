using Xunit;
using Microsoft.Extensions.DependencyInjection;
using SecurityScanFunction.Tests.Models;
using SecurityScanFunction.Tests.Interfaces;
using SecurityScanFunction.Tests.Services;

namespace SecurityScanFunction.Tests
{
    public class SecurityScannerTests : TestBase
    {
        private readonly IServiceProvider _serviceProvider;

        public SecurityScannerTests()
        {
            // Add the test scanner implementation
            Services.AddScoped<ICosmosDBSecurityScanner, TestCosmosDBSecurityScanner>();
            _serviceProvider = Services.BuildServiceProvider();
        }

        [Fact]
        public async Task ScanShouldDetectPublicNetworkAccess()
        {
            // Arrange
            var scanner = _serviceProvider.GetRequiredService<ICosmosDBSecurityScanner>();

            // Act
            var results = await scanner.ScanAsync("test-account");

            // Assert
            Assert.NotNull(results);
            Assert.NotEmpty(results.Issues);
            var networkIssue = Assert.Single(results.Issues, i => 
                i.Category == "NetworkSecurity" && 
                i.Description.Contains("Public network access"));
            Assert.Equal(IssueSeverity.High, networkIssue.Severity);
        }
    }
}