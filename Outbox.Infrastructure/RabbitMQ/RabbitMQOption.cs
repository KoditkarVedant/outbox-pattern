namespace Outbox.Infrastructure.RabbitMQ;

public class RabbitMQOption
{
    public const string RabbitMQ = "RabbitMQ";

    public string Hostname { get; init; }
    public int RetryCount { get; init; }
};