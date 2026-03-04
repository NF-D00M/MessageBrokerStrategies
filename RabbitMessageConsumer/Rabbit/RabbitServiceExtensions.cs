using RabbitMQ.Client;

public static class RabbitServiceExtensions
{
    private static IConnection? _connection;

    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, string hostName = "127.0.0.1")
    {
        services.AddSingleton(sp =>
        {
            ConnectionFactory factory = new ConnectionFactory()
            {
                HostName = hostName
            };

            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            return _connection;
        });

        return services;
    }

    public static IConnection? GetConnection() => _connection;
}