using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Outbox.Core;

namespace Outbox.Infrastructure;

public class PeriodicMessagePublisher : PeriodicTask<PeriodicMessagePublisher>
{
    private readonly Partition _partition;
    private readonly IServiceProvider _serviceProvider;
    public override TimeSpan IntervalBetweenExecutions { get; } = TimeSpan.FromSeconds(10);

    public PeriodicMessagePublisher(
        Partition partition,
        ILogger<PeriodicMessagePublisher> logger,
        IServiceProvider serviceProvider) : base(logger)
    {
        _partition = partition;
        _serviceProvider = serviceProvider;
    }

    protected override async Task Execute(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation(
            "START: Executing PeriodicMessagePublisher for Partition {PartitionKey}",
            _partition.PartitionKey
        );

        using var scope = _serviceProvider.CreateScope();
        var messagePublisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();
        await messagePublisher.PublishPendingMessages(_partition);
        Logger.LogInformation(
            "COMPLETED: Executing PeriodicMessagePublisher for Partition {PartitionKey}",
            _partition.PartitionKey
        );
    }
}