using Confluent.Kafka;
using System.Text.Json;

public class KafkaConsumerService : BackgroundService
{
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly IConfiguration _config;

    public KafkaConsumerService(ILogger<KafkaConsumerService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _config = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _config["Kafka:BootstrapServers"],
            GroupId = _config["Kafka:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Earliest, 
            EnableAutoCommit = true
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe("test-topic");

        _logger.LogInformation("Waiting for messages on 'test-topic'...");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Long polling
                var result = consumer.Consume(stoppingToken);

                if (result != null)
                {
                    ProcessMessage(result);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        finally
        {
            consumer.Close(); // Ensures the group knows this consumer is leaving
        }
    }

    private void ProcessMessage(ConsumeResult<string, string> result)
    {
        string displayBody;
        try
        {
            // Attempt to Pretty-Print JSON
            using var doc = JsonDocument.Parse(result.Message.Value);
            displayBody = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            // Fallback for plain text (like your "Hello" test)
            displayBody = result.Message.Value;
        }

        _logger.LogInformation("\n[MESSAGE RECEIVED]\nKey: {Key}\nPartition: {Partition}\nOffset: {Offset}\nPayload:\n{Payload}",
            result.Message.Key ?? "NULL", result.Partition.Value, result.Offset.Value, displayBody);
    }
}