using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Outbox.Core;
using Outbox.Infrastructure.RabbitMQ;
using RabbitMQ.Client;

namespace Outbox.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOutbox(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OutboxContext>();
        services.AddSingleton<IMessagePublisherService, MessagePublisherService>();
        services.AddSingleton<IMessageLocker, DistributedMessageLocker>();
        services.AddTransient<IMessagePublisher, MessagePublisher>();
        services.AddTransient<IMessageRepository, MessageRepository>();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "CacheInstance";
        });
        
        services.AddOptions<RabbitMQOption>().Bind(configuration.GetSection(RabbitMQOption.RabbitMQ));
        services.AddSingleton<IRabbitMQPersistentConnection, DefaultRabbitMQPersistentConnection>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();
            var options = sp.GetRequiredService<IOptions<RabbitMQOption>>().Value;
            var factory = new ConnectionFactory()
            {
                HostName = options.Hostname
            };

            return new DefaultRabbitMQPersistentConnection(logger, factory);
        });
        services.AddTransient<IMessagePublisherClient, RabbitMQMessagePublisherClient>();

        return services;
    }
}