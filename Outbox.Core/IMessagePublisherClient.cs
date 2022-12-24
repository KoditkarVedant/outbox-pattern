namespace Outbox.Core;

public interface IMessagePublisherClient
{
    Task PublishAsync(Message message);
}