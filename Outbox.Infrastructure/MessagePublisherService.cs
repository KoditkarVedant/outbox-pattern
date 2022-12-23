using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Outbox.Core;

namespace Outbox.Infrastructure;

public class MessagePublisherService : PeriodicTask<MessagePublisherService>, IMessagePublisherService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Task> _tasks = new();

    public MessagePublisherService(ILogger<MessagePublisherService> logger, IServiceProvider serviceProvider)
        : base(logger)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task Execute(CancellationToken cancellationToken = default)
    {
        var scope = _serviceProvider.CreateScope();
        var messageRepository = scope.ServiceProvider.GetRequiredService<IMessageRepository>();

        var partitions = await messageRepository.GetPartitions();

        foreach (var partition in partitions)
        {
            // TODO: just set to this as non nullable field
            var partitionKey = partition.PartitionKey ?? string.Empty;

            if (_tasks.TryGetValue(partitionKey, out var task))
            {
                if (task.IsFaulted)
                {
                    Logger.LogError(task.Exception!.InnerException, "PeriodicMessagePublisher failed with Error");
                }

                if (!task.IsCompleted)
                {
                    Logger.LogInformation("PeriodicMessagePublisher already registered for Partition {PartitionKey}",
                        partition.PartitionKey
                    );

                    continue;
                }
            }

            Logger.LogInformation("Registered Periodic Message Publisher for Partition {PartitionKey}",
                partition.PartitionKey
            );
            _tasks[partitionKey] = PublishMessages(partition, cancellationToken);
        }
    }

    public override TimeSpan IntervalBetweenExecutions { get; } = TimeSpan.FromSeconds(15);

    private async Task PublishMessages(Partition partition, CancellationToken cancellationToken = default)
    {
        await Task.Delay(5, cancellationToken);
        Logger.LogInformation(
            "START: Executing PeriodicMessagePublisher for Partition {PartitionKey}",
            partition.PartitionKey
        );

        using var scope = _serviceProvider.CreateScope();
        var messagePublisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();
        await messagePublisher.PublishPendingMessages(partition);

        Logger.LogInformation(
            "COMPLETED: Executing PeriodicMessagePublisher for Partition {PartitionKey}",
            partition.PartitionKey
        );
    }
}