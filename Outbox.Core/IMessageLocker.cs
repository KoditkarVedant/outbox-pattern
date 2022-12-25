namespace Outbox.Core;

public interface IMessageLocker
{
    ValueTask<bool> LockMessage(Message message);
}