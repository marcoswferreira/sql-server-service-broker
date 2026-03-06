namespace ServiceBroker.ConsoleApplication.Interfaces;

public interface IMessageHandler
{
    Task HandleMessageAsync(string message, CancellationToken cancellationToken = default);
}
