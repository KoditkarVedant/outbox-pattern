namespace Outbox.Core;

public interface IMessageRepository
{
    Task<Message?> GetMessageToDeliver(Partition partition);
    Task UpdateMessage(Message message);
    Task AddMessageToDeliver(Message message);
    Task<IEnumerable<Partition>> GetPartitions();
}