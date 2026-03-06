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
        // Retrieve queue from config
        string? queueName = _config.GetSection("Rabbit:Exchanges:0:Queues:0").Value;

        // Check if Rabbit is connected
        if (_connection == null || !_connection.IsOpen)
        {
            Console.WriteLine("[CRITICAL ERROR] RabbitMQ Connection failed: The broker is unreachable.");
            return;
        }

        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        try
        {
            // 2. Check if Queue exists (Passive declaration)
            await _channel.QueueDeclarePassiveAsync(queue: queueName, cancellationToken: stoppingToken);
        }
        catch (RabbitMQ.Client.Exceptions.OperationInterruptedException)
        {
            Console.WriteLine($"[CRITICAL ERROR] RabbitMQ Topology Error: The queue '{queueName}' does not exist.");
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CRITICAL ERROR] Unexpected error: {ex.Message}");
            return;
        }

        // Set Quality of Service prefetch for priority processing
        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            byte[] body = ea.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} | " +
                  $"{AppDomain.CurrentDomain.FriendlyName} | " +
                  $"{nameof(ExecuteAsync)} | " +
                  $"Received: {message} | " +
                  $"Priority: {ea.BasicProperties.Priority}");
            await Task.CompletedTask;
        };

        await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: true,
            consumer: consumer,
            cancellationToken: stoppingToken
        );

        // Keep the service alive
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

}