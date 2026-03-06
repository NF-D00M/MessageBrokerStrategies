namespace RabbitMessageBroker.Models
{
    public class Exchange
    {
        public string Name { get; set; } = string.Empty;
        public string Vhost { get; set; }
        public string Type { get; set; }
        public List<string> Queues { get; set; } = new();
    }
}
