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

        // Create Exchange if it doens't exist
        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Fanout,
            durable: true, 
            autoDelete: false);

        byte[] body = message is string str
            ? Encoding.UTF8.GetBytes(str)
            : JsonSerializer.SerializeToUtf8Bytes(message);

        // Set priority in properties
        var properties = new BasicProperties
        {
            Priority = priority,
            Persistent = true
        };

        // Publish message to queue with priority
        await channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: string.Empty,
            mandatory: false,
            basicProperties: properties,
            body: body
        );
    }

   
    public async Task InitializeDefinitionsAsync()
    {
        using IChannel channel = await _connection.CreateChannelAsync();

        // Get list of Exchanges
        var exchanges = _config.GetSection("Rabbit:Exchanges").Get<List<Exchange>>();

        if (exchanges == null) return;

        // Define priority arguments (Max 10)
        Dictionary<string, object> queueArgs = new Dictionary<string, object> { { "x-max-priority", 10 } };

        foreach (Exchange exchange in exchanges)
        {
            // Declare exchange if it doesn't exist
            await channel.ExchangeDeclareAsync(
                exchange: exchange.Name,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false);

            // Loop through each queue defined for this exchange
            if (exchange.Queues != null)
            {
                foreach (string queueName in exchange.Queues)
                {
                    // Declare the Queue with priority support
                    await channel.QueueDeclareAsync(
                        queue: queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: queueArgs
                    );

                    // Bind queue to exchange
                    await channel.QueueBindAsync(
                        queue: queueName,
                        exchange: exchange.Name,
                        routingKey: string.Empty
                    );
                }
            }
        }
    }

}
