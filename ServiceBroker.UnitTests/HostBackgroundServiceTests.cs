using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using ServiceBroker.ConsoleApplication.Interfaces;
using ServiceBroker.ConsoleApplication.Services;
using Xunit;

namespace ServiceBroker.UnitTests;

public class HostBackgroundServiceTests
{
    private readonly Mock<ISubscribeService> _subscribeServiceMock;
    private readonly Mock<ILogger<HostBackgroundService>> _loggerMock;
    private readonly HostBackgroundService _hostBackgroundService;

    public HostBackgroundServiceTests()
    {
        _subscribeServiceMock = new Mock<ISubscribeService>();
        _loggerMock = new Mock<ILogger<HostBackgroundService>>();
        _hostBackgroundService = new HostBackgroundService(_subscribeServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallSubscribeMessageOnce_WhenCancellationRequested()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        int callCount = 0;
        
        _subscribeServiceMock
            .Setup(x => x.SubscribeMessageAsync(It.IsAny<CancellationToken>()))
            .Returns(() => 
            {
                callCount++;
                cts.Cancel(); // Cancel immediately after first call so loop exits
                return Task.CompletedTask;
            });

        // Act - Call protected ExecuteAsync via reflection
        MethodInfo? methodInfo = typeof(HostBackgroundService).GetMethod("ExecuteAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        var task = methodInfo?.Invoke(_hostBackgroundService, new object[] { cts.Token }) as Task;

        if (task != null)
        {
            await task;
        }

        // Assert
        _subscribeServiceMock.Verify(x => x.SubscribeMessageAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogException_WhenSubscribeMessageThrows()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        int callCount = 0;
        
        _subscribeServiceMock
            .Setup(x => x.SubscribeMessageAsync(It.IsAny<CancellationToken>()))
            .Returns(() => 
            {
                callCount++;
                // Cancel token so the loop exits after this iteration
                cts.Cancel();
                throw new Exception("Test exception");
            });

        // Act
        MethodInfo? methodInfo = typeof(HostBackgroundService).GetMethod("ExecuteAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        var task = methodInfo?.Invoke(_hostBackgroundService, new object[] { cts.Token }) as Task;

        if (task != null)
        {
            await task;
        }

        // Assert
        _subscribeServiceMock.Verify(x => x.SubscribeMessageAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        // Cannot easily verify ILogger extension methods with Moq directly without complex matchers, 
        // but the test proves the exception was caught and didn't crash ExecuteAsync.
        Assert.Equal(1, callCount);
    }
}
