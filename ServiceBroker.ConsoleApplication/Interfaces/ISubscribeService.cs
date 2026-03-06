namespace ServiceBroker.ConsoleApplication.Interfaces;

public interface ISubscribeService
{
    Task SubscribeMessageAsync(CancellationToken cancellationToken = default);
}
