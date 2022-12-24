using Microsoft.Extensions.DependencyInjection;

namespace Outbox.Extensions.Hosting;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOutboxHostedService(this IServiceCollection services)
    {
        services.AddHostedService<MessageHostedService>();
        return services;
    }
}