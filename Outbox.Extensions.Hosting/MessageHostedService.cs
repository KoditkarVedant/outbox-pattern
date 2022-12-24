using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Outbox.Core;

namespace Outbox.Extensions.Hosting;

public class MessageHostedService : BackgroundService
{
    private readonly ILogger<MessageHostedService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHostApplicationLifetime _lifetime;

    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromSeconds(5));

    public MessageHostedService(
        ILogger<MessageHostedService> logger,
        IServiceScopeFactory scopeFactory,
        IHostApplicationLifetime lifetime)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!await WaitForAppToStart(_lifetime, stoppingToken))
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested && await _periodicTimer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var messagePublisherService = scope.ServiceProvider.GetRequiredService<IMessagePublisherService>();
                await messagePublisherService.StartAsync(stoppingToken);

                _logger.LogInformation("MessagePublisherService has started...");
                break;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error starting MessagePublisherService...");
            }
        }
    }

    private static async Task<bool> WaitForAppToStart(IHostApplicationLifetime lifetime,
        CancellationToken stoppingToken)
    {
        var appStartedSource = new TaskCompletionSource();
        var hostCancelledSource = new TaskCompletionSource();

        await using var cancellationTokenRegistration =
            lifetime.ApplicationStarted.Register(() => appStartedSource.SetResult());
        await using var tokenRegistration = stoppingToken.Register(() => hostCancelledSource.SetResult());

        var completedTask = await Task.WhenAny(appStartedSource.Task, hostCancelledSource.Task).ConfigureAwait(false);

        return completedTask == appStartedSource.Task;
    }
}