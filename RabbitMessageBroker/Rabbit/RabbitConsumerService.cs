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
                await _channel.ExchangeDeclareAsync(
                    exchange: exchange.Name,
                    type: ExchangeType.Fanout,
                    durable: false,
                    autoDelete: false,
                    cancellationToken: stoppingToken
                );

                foreach (string queue in exchange.Queues)
                {

                    Dictionary<string, object> args = new Dictionary<string, object> { { "x-max-priority", 10 } };

                    await _channel.QueueDeclareAsync(
                        queue: queue,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: args, // Add the args
                        cancellationToken: stoppingToken
                    );

                    await _channel.QueueBindAsync(
                        queue: queue,
                        exchange: exchange.Name,
                        routingKey: "", 
                        cancellationToken: stoppingToken
                    );

                    // 2. Add QoS to ensure priority 
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
                            $"ReceivedAsync : Exchange: {ea.Exchange}, Queue: {queue}, Message: {message}, Priority {ea.BasicProperties.Priority}");
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

            // 3. KEEP THE SERVICE ALIVE
            // Without this, the method ends and the consumer is killed.
            await Task.Delay(Timeout.Infinite, stoppingToken);
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