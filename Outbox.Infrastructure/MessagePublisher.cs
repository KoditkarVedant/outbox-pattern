using System.Text.Json;
using Microsoft.Extensions.Logging;
using Outbox.Core;

namespace Outbox.Infrastructure;

public class MessagePublisher : IMessagePublisher
{
    private readonly ILogger<MessagePublisher> _logger;
    private readonly IMessageRepository _messageRepository;
    private readonly IMessageLocker _messageLocker;

    private static readonly JsonSerializerOptions Settings = new() { WriteIndented = true };

    public MessagePublisher(
        ILogger<MessagePublisher> logger,
        IMessageRepository messageRepository,
        IMessageLocker messageLocker)
    {
        _logger = logger;
        _messageRepository = messageRepository;
        _messageLocker = messageLocker;
    }

    public async Task PublishPendingMessages(Partition partition)
    {
        while (true)
        {
            var message = await _messageRepository.GetMessageToDeliver(partition);

            if (message is null)
            {
                _logger.LogInformation(
                    "No pending messages to deliver for partition {PartitionKey}",
                    partition.PartitionKey
                );

                break;
            }

            var lockAcquired = await _messageLocker.LockMessage(message);
            if (lockAcquired)
            {
                _logger.LogWarning(
                    "Acquired lock on message \n{MessageJson}",
                    JsonSerializer.Serialize(message, Settings)
                );

                message.DeliveryStatus = MessageDeliveryStatus.Picked;
                await _messageRepository.UpdateMessage(message);
                _logger.LogWarning("Message Picked \n{MessageJson}", JsonSerializer.Serialize(message, Settings));

                // TODO: Add PublisherClient

                message.DeliveryStatus = MessageDeliveryStatus.Sent;
                message.PublishedDateTime = DateTime.UtcNow;
                await _messageRepository.UpdateMessage(message);
                _logger.LogWarning("Message Sent \n{MessageJson}", JsonSerializer.Serialize(message, Settings));
            }
            else
            {
                _logger.LogWarning(
                    "Unable to acquire lock on message {MessageJson}",
                    JsonSerializer.Serialize(message, Settings)
                );
            }
        }
    }
}