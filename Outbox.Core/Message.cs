namespace Outbox.Core;

public class Message
{
    public Guid Id { get; set; }
    public string? PartitionKey { get; set; }
    public string Content { get; set; } = null!;
    public MessageDeliveryStatus DeliveryStatus { get; set; }
    public int RetryCount { get; set; }
    public DateTime ReceivedDateTime { get; set; }
    public DateTime? PublishedDateTime { get; set; }
}