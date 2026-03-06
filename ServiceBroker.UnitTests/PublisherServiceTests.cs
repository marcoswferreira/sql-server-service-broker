using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ServiceBroker.ConsoleApplication.Configurations;
using ServiceBroker.ConsoleApplication.Interfaces;
using ServiceBroker.ConsoleApplication.Services;
using Xunit;

namespace ServiceBroker.UnitTests;

public class PublisherServiceTests
{
    private readonly Mock<IDbConnectionFactory> _connectionFactoryMock;
    private readonly Mock<IOptions<ServiceBrokerOptions>> _optionsMock;
    private readonly Mock<ILogger<PublisherService>> _loggerMock;
    private readonly PublisherService _publisherService;

    public PublisherServiceTests()
    {
        _connectionFactoryMock = new Mock<IDbConnectionFactory>();
        _optionsMock = new Mock<IOptions<ServiceBrokerOptions>>();
        _loggerMock = new Mock<ILogger<PublisherService>>();
        
        var options = new ServiceBrokerOptions 
        {
            Queue = "TestQueue",
            Service = "TestService",
            Contract = "TestContract",
            MessageType = "TestMessage"
        };
        _optionsMock.Setup(x => x.Value).Returns(options);

        _publisherService = new PublisherService(
            _connectionFactoryMock.Object, 
            _optionsMock.Object, 
            _loggerMock.Object);
    }

    [Fact]
    public void GetQuery_ShouldReturnCorrectlyFormattedQueryString()
    {
        // Use reflection to access the private GetQuery method
        MethodInfo? methodInfo = typeof(PublisherService).GetMethod("GetQuery", BindingFlags.NonPublic | BindingFlags.Instance);
        
        Assert.NotNull(methodInfo);

        // Act
        string? result = methodInfo?.Invoke(_publisherService, null) as string;

        // Assert
        Assert.NotNull(result);
        Assert.Contains("BEGIN DIALOG @dialog_handle", result);
        Assert.Contains("FROM SERVICE TestService", result);
        Assert.Contains("TO SERVICE 'TestService', 'CURRENT DATABASE'", result);
        Assert.Contains("ON CONTRACT TestContract", result);
        Assert.Contains("SEND ON CONVERSATION @dialog_handle", result);
        Assert.Contains("MESSAGE TYPE TestMessage(@message_body);", result);
    }
}
