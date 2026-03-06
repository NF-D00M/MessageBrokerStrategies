using RabbitMessageBroker.Models;
using RabbitMessageBroker.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

public class RabbitBroker : IMessageBroker
{
    private readonly IConfiguration _config;
    private readonly IConnection _connection;

    public RabbitBroker(IConfiguration config, IConnection connection)
    {
        _config = config;
        _connection = connection;
    }

    public async Task PublishAsync<T>(string exchangeName, byte priority, T message)
    {
        using IChannel channel = await _connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: "fanout", 
            durable: false,
            autoDelete: false
        );

        byte[] body = message is string str
            ? Encoding.UTF8.GetBytes(str)
            : JsonSerializer.SerializeToUtf8Bytes(message);

        BasicProperties properties = new BasicProperties
        {
            Priority = priority,
            Persistent = true 
        };

        await channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: string.Empty,
            mandatory: false,
            basicProperties: properties,
            body: body
        );
    }

    public async Task SubscribeAsync<T>(string destination, Func<T, BasicDeliverEventArgs, Task> handler)
    {
        IChannel channel = await _connection.CreateChannelAsync();

        var exchanges = _config.GetSection("Rabbit:Exchanges").Get<List<Exchange>>();

        await channel.QueueDeclareAsync(
            queue: destination,
            durable: false,
            exclusive: false,
            autoDelete: false);

        foreach (var exchange in exchanges)
        {
            await channel.ExchangeDeclareAsync(
                exchange: exchange.Name,
                type: "fanout", 
                durable: false,
                autoDelete: false);

            await channel.QueueBindAsync(
                queue: destination,
                exchange: exchange.Name,
                routingKey: "" 
            );
        }

        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            byte[] body = ea.Body.ToArray();
            T? message;
            if (typeof(T) == typeof(string))
            {
                object? str = Encoding.UTF8.GetString(body);
                message = (T?)str;
            }
            else
            {
                string messageJson = Encoding.UTF8.GetString(body);
                message = JsonSerializer.Deserialize<T>(messageJson);
            }

            if (message != null)
            {
                await handler(message, ea);
            }
        };

        await channel.BasicConsumeAsync(queue: destination, autoAck: true, consumer: consumer);
    }
}