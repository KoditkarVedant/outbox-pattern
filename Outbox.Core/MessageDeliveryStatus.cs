namespace Outbox.Core;

public enum MessageDeliveryStatus
{
    NotSent = 0,
    Picked = 1,
    Sent = 2,
    Failed = 3
}