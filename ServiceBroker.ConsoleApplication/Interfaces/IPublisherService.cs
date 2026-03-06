namespace ServiceBroker.ConsoleApplication.Interfaces;

public interface IPublisherService
{
    Task PublishMessageAsync(string message = "Hello World!", CancellationToken cancellationToken = default);
}
