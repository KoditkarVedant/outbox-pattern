using System.Text;
using Outbox.Core;
using RabbitMQ.Client;

namespace Outbox.Infrastructure.RabbitMQ;

public class RabbitMQMessagePublisherClient : IMessagePublisherClient
{
    private readonly IRabbitMQPersistentConnection _connection;

    public RabbitMQMessagePublisherClient(IRabbitMQPersistentConnection connection)
    {
        _connection = connection;
    }

    public async Task PublishAsync(Message message)
    {
        await Task.CompletedTask;

        try
        {
            using var channel = _connection.CreateModel();
            channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Fanout);

            var body = Encoding.UTF8.GetBytes(message.Content);
            channel.BasicPublish("logs", "", null, body);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        

        
    }
}