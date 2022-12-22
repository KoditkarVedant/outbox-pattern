namespace Outbox.Core;

public interface IMessagePublisher
{
    Task PublishPendingMessages(Partition partition);
}