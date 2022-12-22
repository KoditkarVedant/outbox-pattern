namespace Outbox.Core;

public interface IPeriodicMessagePublisherStore
{
    void Add(string partitionKey, IPeriodicTask task);
    bool Exists(string partitionKey);
}