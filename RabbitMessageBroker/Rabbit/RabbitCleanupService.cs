using RabbitMessageBroker.Models;
using System.Net.Http.Headers;
using System.Text.Json;

public class RabbitCleanupService
{
    private readonly HttpClient _httpClient;

    public RabbitCleanupService(string baseUrl, string username, string password)
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(baseUrl);
        byte[] byteArray = System.Text.Encoding.ASCII.GetBytes($"{username}:{password}");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
    }

    public async Task CleanupInactiveAsync()
    {
        // Cleanup Queues
        HttpResponseMessage queuesResponse = await _httpClient.GetAsync("/api/queues");
        string queuesJson = await queuesResponse.Content.ReadAsStringAsync();
        List<Queue>? queues = JsonSerializer.Deserialize<List<Queue>>(queuesJson);

        foreach (Queue queue in queues)
        {
            if (queue.consumers == 0 && queue.messages == 0)
            {
                await _httpClient.DeleteAsync($"/api/queues/{Uri.EscapeDataString(queue.vhost)}/{Uri.EscapeDataString(queue.name)}");
            }
        }

        // Cleanup Exchanges 
        HttpResponseMessage exchangesResponse = await _httpClient.GetAsync("/api/exchanges");
        string exchangesJson = await exchangesResponse.Content.ReadAsStringAsync();
        List<Exchange>? exchanges = JsonSerializer.Deserialize<List<Exchange>>(exchangesJson);

        foreach (Exchange exchange in exchanges)
        {
            if (!exchange.Name.StartsWith("amq.") && exchange.Name != "" && exchange.Type == "fanout")
            {
                await _httpClient.DeleteAsync($"/api/exchanges/{Uri.EscapeDataString(exchange.Vhost)}/{Uri.EscapeDataString(exchange.Name)}");
            }
        }
    }
}