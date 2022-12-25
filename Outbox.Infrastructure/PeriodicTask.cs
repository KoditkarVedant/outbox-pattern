using Microsoft.Extensions.Logging;
using Outbox.Core;

namespace Outbox.Infrastructure;

public abstract class PeriodicTask<T> : IPeriodicTask
{
    protected readonly ILogger<PeriodicTask<T>> Logger;

    public abstract TimeSpan IntervalBetweenExecutions { get; }
    private PeriodicTimer _periodicTimer = null!;
    private Task? _executeTask;
    private CancellationTokenSource? _stoppingCts;

    protected PeriodicTask(ILogger<PeriodicTask<T>> logger)
    {
        Logger = logger;
    }

    public virtual Task StartAsync(CancellationToken cancellationToken)
    {
        _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _executeTask = ExecuteAsync(_stoppingCts.Token);

        return _executeTask.IsCompleted ? _executeTask : Task.CompletedTask;
    }

    public virtual async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_executeTask == null)
        {
            return;
        }

        try
        {
            _stoppingCts!.Cancel();
        }
        finally
        {
            // Wait until the task completes or the stop token triggers
            await Task.WhenAny(_executeTask, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
        }
    }

    private async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _periodicTimer = new PeriodicTimer(IntervalBetweenExecutions);

        while (stoppingToken.IsCancellationRequested is not true &&
               await _periodicTimer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await Execute(stoppingToken);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Something went wrong when running the PeriodicTask");
            }
        }
    }

    protected abstract Task Execute(CancellationToken cancellationToken = default);


    public void Dispose()
    {
        _stoppingCts?.Cancel();
    }
}