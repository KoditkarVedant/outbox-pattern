using Microsoft.EntityFrameworkCore;
using Outbox.Core;

namespace Outbox.Infrastructure;

public class MessageRepository : IMessageRepository
{
    private readonly DbContext _context;

    public MessageRepository(OutboxContext context)
    {
        _context = context;
    }

    public async Task<Message?> GetMessageToDeliver(Partition partition)
    {
        var messages = _context.Set<Message>().OrderBy(x => x.ReceivedDateTime);
            
        var message = await messages.FirstOrDefaultAsync(
            x => x.PartitionKey == partition.PartitionKey &&
                 (x.DeliveryStatus != MessageDeliveryStatus.Sent && x.DeliveryStatus != MessageDeliveryStatus.Failed)
        );

        return message;
    }

    public async Task UpdateMessage(Message message)
    {
        _context.Set<Message>().Update(message);
        await _context.SaveChangesAsync();
    }

    public async Task AddMessageToDeliver(Message message)
    {
        var partitionExists = await _context.Set<Partition>().AnyAsync(x => x.PartitionKey == message.PartitionKey);
        if (!partitionExists)
        {
            await _context.Set<Partition>().AddAsync(new Partition()
            {
                PartitionKey = message.PartitionKey
            });
        }

        await _context.Set<Message>().AddAsync(message);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Partition>> GetPartitions()
    {
        return await _context.Set<Partition>().ToListAsync();
    }
}