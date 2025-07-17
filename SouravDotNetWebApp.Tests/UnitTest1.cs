using Microsoft.Extensions.Logging;
using Moq;
using SouravDotNetWebApp.Pages;
using Xunit;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        // Create a mock logger
        var mockLogger = new Mock<ILogger<IndexModel>>();

        // Instantiate IndexModel with the mock logger
        var indexModel = new IndexModel(mockLogger.Object);

        // Your test logic here
        Assert.NotNull(indexModel);
    }
}