using System.Text.Json;
using Outbox.Core;
using Outbox.Infrastructure;
using Outbox.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

builder.Services.AddDbContext<OutboxContext>();

builder.Services.AddSingleton<IMessagePublisherService, MessagePublisherService>();
builder.Services.AddSingleton<IMessageLocker, DistributedMessageLocker>();

builder.Services.AddTransient<IMessagePublisher, MessagePublisher>();
builder.Services.AddTransient<IMessageRepository, MessageRepository>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "CacheInstance";
});

builder.Services.AddHostedService<MessageHostedService>();

var app = builder.Build();

app.MapGet("/", () => "Running...");

app.MapPost("/publish-messages", (ILogger<Program> logger,
    IList<PublishMessage> messages,
    IMessageRepository repository) =>
{
    foreach (var message in messages)
    {
        logger.LogInformation("Saving message in Partition {PartitionKey}", message.PartitionKey);
        repository.AddMessageToDeliver(new Message()
        {
            PartitionKey = message.PartitionKey,
            Content = JsonSerializer.Serialize(message.Content),
            ReceivedDateTime = DateTime.UtcNow
        });
        logger.LogInformation("Saved message in Partition {PartitionKey}", message.PartitionKey);
    }
});

app.Run();

public record PublishMessage(string PartitionKey, object Content);