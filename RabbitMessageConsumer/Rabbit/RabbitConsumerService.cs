using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Hosting;
using System.Text;

public class RabbitConsumerService : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly IConnection _connection;
    private IChannel? _channel;

    public RabbitConsumerService(IConfiguration config, IConnection connection)
    {
        _config = config;
        _connection = connection;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string? exchangeName = _config.GetSection("Rabbit:Exchanges:0:Name").Value;
        string? queueName = _config.GetSection("Rabbit:Exchanges:0:Queues:0").Value;
        string exchangeType = "fanout"; 

        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        // Declare the exchange
        await _channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: exchangeType,
            durable: false, 
            autoDelete: false,
            cancellationToken: stoppingToken
        );

        Dictionary<string, object> args = new Dictionary<string, object> { { "x-max-priority", 10 } };

        // Declare the queue
        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: args, // <--- Pass the dictionary here
            cancellationToken: stoppingToken
        );

        // Bind the queue to the exchange
        await _channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: "", 
            cancellationToken: stoppingToken
        );

        await _channel.BasicQosAsync(0, 1, false, stoppingToken);

        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            byte[] body = ea.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"" +
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} | " +
                $"{AppDomain.CurrentDomain.FriendlyName} | " +
                $"{nameof(RabbitConsumerService)} | " +
                $"ReceivedAsync : Exchange: {ea.Exchange}, Queue: {queueName}, Message: {message}, Priority {ea.BasicProperties.Priority}");
            await Task.CompletedTask;
        };

        await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: true,
            consumer: consumer,
            cancellationToken: stoppingToken
        );

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}