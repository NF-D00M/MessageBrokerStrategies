using RabbitMQ.Client;
using RabbitMessageBroker.RabbitMQ; 

public static class RabbitServiceExtensions
{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, string hostName = "127.0.0.1")
    {
        services.AddSingleton(sp =>
        {
            ConnectionFactory factory = new ConnectionFactory()
            {
                HostName = hostName
            };

            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        });

        services.AddSingleton<IMessageBroker, RabbitBroker>();

        return services;
    }
}