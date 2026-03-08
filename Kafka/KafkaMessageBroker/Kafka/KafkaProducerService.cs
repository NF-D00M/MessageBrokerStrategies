using Confluent.Kafka;
using System.Text.Json;

namespace KafkaMessageBroker.Kafka
{
    public class KafkaProducerService
    {
        private readonly IProducer<string, string> _producer;

        public KafkaProducerService(IConfiguration _config)
        {
            ProducerConfig config = new ProducerConfig
            {
                BootstrapServers = _config["Kafka:BootstrapServers"],
                Acks = Acks.All,
                EnableIdempotence = true
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task<DeliveryResult<string, string>> PublishAsync(string topic, string key, object message)
        {
            var payload = JsonSerializer.Serialize(message);
            return await _producer.ProduceAsync(topic, new Message<string, string>
            {
                Key = key,
                Value = payload
            });
        }

        public void Dispose()
        {
            _producer.Flush(TimeSpan.FromSeconds(10));
            _producer.Dispose();
        }
    }
}
