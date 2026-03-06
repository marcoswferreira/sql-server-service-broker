using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServiceBroker.ConsoleApplication.Interfaces;

namespace ServiceBroker.ConsoleApplication.Services;

public class HostBackgroundService : BackgroundService
{
    private readonly ISubscribeService _subscribeService;
    private readonly ILogger<HostBackgroundService> _logger;
    public HostBackgroundService(ISubscribeService subscribeService, ILogger<HostBackgroundService> logger)
    {
        _subscribeService = subscribeService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _subscribeService.SubscribeMessageAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on subscribe message.");
            }
        }
    }
}