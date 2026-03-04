using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Hosting;
using System.Text;

public class RabbitConsumerService : BackgroundService
{
    private readonly IConnection _connection;
    private IChannel? _channel;

    public RabbitConsumerService(IConnection connection)
    {
        _connection = connection;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const string exchangeName = "Test-Exchange-1";
        const string queueName = "Test-Queue-1.1a";
        const string exchangeType = "fanout"; 

        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        // Declare the exchange
        await _channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: exchangeType,
            durable: false, 
            autoDelete: false,
            cancellationToken: stoppingToken
        );

        // Declare the queue
        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken
        );

        // Bind the queue to the exchange
        await _channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: "", 
            cancellationToken: stoppingToken
        );

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} | {AppDomain.CurrentDomain.FriendlyName} | {nameof(RabbitConsumerService)} | ReceivedAsync : Exchange: {ea.Exchange}, Queue: {queueName}, Message: {message}");
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