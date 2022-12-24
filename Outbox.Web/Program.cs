using System.Text.Json;
using Outbox.Core;
using Outbox.Extensions.Hosting;
using Outbox.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

builder.Services.AddOutbox(builder.Configuration);
builder.Services.AddOutboxHostedService();

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