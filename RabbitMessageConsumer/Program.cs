using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRabbitMQ("127.0.0.1");
builder.Services.AddHostedService<RabbitConsumerService>();

builder.Build().Run();