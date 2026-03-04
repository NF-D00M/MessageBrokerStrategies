namespace RabbitMessageBroker.Models
{
    public class Queue
    {
        public string name { get; set; }
        public string vhost { get; set; }
        public int consumers { get; set; }
        public int messages { get; set; }
    }
}
