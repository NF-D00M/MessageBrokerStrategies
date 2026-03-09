using KafkaMessageConsumer.Kafka;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddHostedService<KafkaConsumerService>();

WebApplication app = builder.Build();

app.Run();
