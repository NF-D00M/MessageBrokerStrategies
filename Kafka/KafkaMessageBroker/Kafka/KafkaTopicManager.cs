using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace KafkaMessageBroker.Kafka
{
    public class KafkaTopicManager
    {
        private readonly AdminClientConfig adminClientConfig;

        public KafkaTopicManager(IConfiguration _config)
        {
            adminClientConfig = new AdminClientConfig
            {
                BootstrapServers = _config["Kafka:BootstrapServers"],
            };
        }

        public async Task CreateTopicIfNotExists(string topicName, int partitions = 3)
        {
            using var adminClient = new AdminClientBuilder(adminClientConfig).Build();
            try
            {
                await adminClient.CreateTopicsAsync(new TopicSpecification[]
                {
                new TopicSpecification { Name = topicName, NumPartitions = partitions, ReplicationFactor = 1 }
                });

                Console.WriteLine($"Topic '{topicName}' created.");
            }
            catch (CreateTopicsException e) when (e.Results[0].Error.Code == ErrorCode.TopicAlreadyExists)
            {
                Console.WriteLine($"Topic '{topicName}' already exists.");
            }
        }
    }
}
