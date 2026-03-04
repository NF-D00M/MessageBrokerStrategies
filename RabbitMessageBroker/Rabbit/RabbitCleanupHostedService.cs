public class RabbitCleanupHostedService : IHostedService
{
    private readonly IConfiguration _config;

    public RabbitCleanupHostedService(IConfiguration config)
    {
        _config = config;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        string? baseUrl = _config["Rabbit:ManagementUrl"];
        string? username = _config["Rabbit:ManagementUser"];
        string? password = _config["Rabbit:ManagementPassword"];

        var cleanupService = new RabbitCleanupService(baseUrl, username, password);
        await cleanupService.CleanupInactiveAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}