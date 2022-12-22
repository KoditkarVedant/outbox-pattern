using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Outbox.Core;

namespace Outbox.Infrastructure;

public class MessagePublisherService : PeriodicTask<MessagePublisherService>, IMessagePublisherService
{
    private readonly IServiceProvider _serviceProvider;

    public MessagePublisherService(ILogger<MessagePublisherService> logger, IServiceProvider serviceProvider)
        : base(logger)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task Execute(CancellationToken cancellationToken = default)
    {
        var scope = _serviceProvider.CreateScope();
        var messageRepository = scope.ServiceProvider.GetRequiredService<IMessageRepository>();
        var periodicMessagePublisherStore =
            scope.ServiceProvider.GetRequiredService<IPeriodicMessagePublisherStore>();

        var partitions = await messageRepository.GetPartitions();

        foreach (var partition in partitions)
        {
            // TODO: just set to this as non nullable field
            var partitionKey = partition.PartitionKey ?? string.Empty;
            if (periodicMessagePublisherStore.Exists(partitionKey))
            {
                Logger.LogInformation("PeriodicMessagePublisher already registered for Partition {PartitionKey}",
                    partition.PartitionKey
                );
                continue;
            }

            var publisher = new PeriodicMessagePublisher(
                partition,
                _serviceProvider.GetRequiredService<ILogger<PeriodicMessagePublisher>>(),
                _serviceProvider
            );
            periodicMessagePublisherStore.Add(partitionKey, publisher);
            _ = publisher.StartAsync(cancellationToken);

            Logger.LogInformation("Registered Periodic Message Publisher for Partition {PartitionKey}",
                partition.PartitionKey
            );
        }
    }

    public override TimeSpan IntervalBetweenExecutions { get; } = TimeSpan.FromSeconds(15);
}