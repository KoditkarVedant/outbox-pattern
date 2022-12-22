namespace Outbox.Core;

public class Partition
{
    public Guid Id { get; set; }
    public string? PartitionKey { get; set; }
}