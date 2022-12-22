namespace Outbox.Core;

public interface IMessageLocker
{
    Task<bool> LockMessage(Message message);
}