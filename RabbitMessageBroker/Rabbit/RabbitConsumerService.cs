using System.Text;
using RabbitMessageBroker.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMessageBroker.RabbitMQ
{
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
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

            List<Exchange>? exchanges = _config.GetSection("Rabbit:Exchanges").Get<List<Exchange>>();
            foreach (Exchange exchange in exchanges)
            {
                // Exchange
                await _channel.ExchangeDeclareAsync(
                    exchange: exchange.Name,
                    type: "fanout",
                    durable: false,
                    autoDelete: false,
                    cancellationToken: stoppingToken
                );
                
                foreach (string queue in exchange.Queues)
                {
                    // Queue
                    await _channel.QueueDeclareAsync(
                        queue: queue,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null,
                        cancellationToken: stoppingToken
                    );

                    // Bind the queue to the exchange
                    await _channel.QueueBindAsync(
                        queue: queue,
                        exchange: exchange.Name,
                        routingKey: queue,
                        cancellationToken: stoppingToken
                    );

                    // Consumer for each queue
                    AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_channel);

                    consumer.ReceivedAsync += async (model, ea) =>
                    {
                        byte[] body = ea.Body.ToArray();
                        string message = Encoding.UTF8.GetString(body);

                        Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} | {AppDomain.CurrentDomain.FriendlyName} | {nameof(RabbitConsumerService)} | ReceivedAsync : Exchange: {ea.Exchange}, Queue: {queue}, Message: {message}");
                        await Task.CompletedTask;
                    };

                    await _channel.BasicConsumeAsync(
                        queue: queue,
                        autoAck: true,
                        consumer: consumer,
                        cancellationToken: stoppingToken
                    );
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel != null)
            {
                await _channel.CloseAsync(cancellationToken);
            }
            await base.StopAsync(cancellationToken);
        }
    }
}