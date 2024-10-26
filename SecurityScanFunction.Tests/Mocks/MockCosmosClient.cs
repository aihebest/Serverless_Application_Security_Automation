using Moq;
using Microsoft.Azure.Cosmos;
using SecurityScanFunction.Tests.TestData;

namespace SecurityScanFunction.Tests.Mocks
{
    public static class MockCosmosClient
    {
        public static Mock<CosmosClient> GetMockClient()
        {
            var mockClient = new Mock<CosmosClient>();
            var testAccount = SecurityTestData.GetTestAccount();

            // Mock the necessary Cosmos DB client behaviors
            mockClient
                .Setup(c => c.GetAccountSettingsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(testAccount);

            return mockClient;
        }
    }
}