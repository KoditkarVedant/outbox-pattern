using Outbox.Core;

namespace Outbox.Infrastructure;

public class PeriodicMessagePublisherStore : IPeriodicMessagePublisherStore
{
    private readonly Dictionary<string, IPeriodicTask> _partitionMessagePublishers = new();

    public void Add(string partitionKey, IPeriodicTask task)
    {
        _partitionMessagePublishers.Add(partitionKey, task);
    }

    public bool Exists(string partitionKey)
    {
        return _partitionMessagePublishers.ContainsKey(partitionKey);
    }
}