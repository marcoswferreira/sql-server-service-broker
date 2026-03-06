using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ServiceBroker.ConsoleApplication.Configurations;
using ServiceBroker.ConsoleApplication.Interfaces;
using ServiceBroker.ConsoleApplication.Services;
using Xunit;

namespace ServiceBroker.UnitTests;

public class SubscriberServiceTests
{
    private readonly Mock<IDbConnectionFactory> _connectionFactoryMock;
    private readonly Mock<IMessageHandler> _messageHandlerMock;
    private readonly Mock<IOptions<ServiceBrokerOptions>> _optionsMock;
    private readonly Mock<ILogger<SubscriberService>> _loggerMock;
    private readonly SubscriberService _subscriberService;

    public SubscriberServiceTests()
    {
        _connectionFactoryMock = new Mock<IDbConnectionFactory>();
        _messageHandlerMock = new Mock<IMessageHandler>();
        _optionsMock = new Mock<IOptions<ServiceBrokerOptions>>();
        _loggerMock = new Mock<ILogger<SubscriberService>>();
        
        var options = new ServiceBrokerOptions { Queue = "TestQueue" };
        _optionsMock.Setup(x => x.Value).Returns(options);

        _subscriberService = new SubscriberService(
            _connectionFactoryMock.Object,
            _messageHandlerMock.Object,
            _optionsMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void GetQuery_ShouldReturnCorrectReceiveQuery()
    {
        // Act
        string result = _subscriberService.GetQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("WAITFOR (", result);
        Assert.Contains("RECEIVE TOP(1)", result);
        Assert.Contains("@dialog_handle = conversation_handle,", result);
        Assert.Contains("@message = message_body", result);
        Assert.Contains("FROM dbo.[TestQueue]", result);
        Assert.Contains("TIMEOUT 60000", result);
    }
}
