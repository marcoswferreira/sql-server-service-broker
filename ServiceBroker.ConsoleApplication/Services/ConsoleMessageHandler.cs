using Microsoft.Extensions.Logging;
using ServiceBroker.ConsoleApplication.Interfaces;

namespace ServiceBroker.ConsoleApplication.Services;

public class ConsoleMessageHandler : IMessageHandler
{
    private readonly ILogger<ConsoleMessageHandler> _logger;

    public ConsoleMessageHandler(ILogger<ConsoleMessageHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Received message from Service Broker: {Message}", message);
        return Task.CompletedTask;
    }
}
