namespace Outbox.Core;

public interface IPeriodicTask
{
    TimeSpan IntervalBetweenExecutions { get; }
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}