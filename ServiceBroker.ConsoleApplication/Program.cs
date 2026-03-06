using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ServiceBroker.ConsoleApplication.Configurations;
using ServiceBroker.ConsoleApplication.Interfaces;

class Program
{
    static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        // Run queue initialization before starting the host
        using (var scope = host.Services.CreateScope())
        {
            var connectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
            var options = scope.ServiceProvider.GetRequiredService<IOptions<ServiceBrokerOptions>>();
            
            DatabaseConfig.StartQueue(connectionFactory, options);
        }

        await host.RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.ResolveDependencies(hostContext.Configuration);
            });
}