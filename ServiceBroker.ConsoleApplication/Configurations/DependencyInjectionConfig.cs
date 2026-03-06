using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceBroker.ConsoleApplication.Interfaces;
using ServiceBroker.ConsoleApplication.Services;

namespace ServiceBroker.ConsoleApplication.Configurations;
public static class DependencyInjectionConfig
{
    public static IServiceCollection ResolveDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        // Options
        services.Configure<ServiceBrokerOptions>(configuration.GetSection(ServiceBrokerOptions.SectionName));

        // Singleton Factory
        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

        // Services
        services.AddScoped<IPublisherService, PublisherService>();
        services.AddScoped<ISubscribeService, SubscriberService>();
        services.AddScoped<IMessageHandler, ConsoleMessageHandler>();
        services.AddHostedService<HostBackgroundService>();

        return services;
    }
}
