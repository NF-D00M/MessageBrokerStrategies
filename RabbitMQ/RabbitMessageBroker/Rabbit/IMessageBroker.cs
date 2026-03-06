
using RabbitMQ.Client.Events;

namespace RabbitMessageBroker.RabbitMQ;

public interface IMessageBroker
{
    Task PublishAsync<T>(string exchangeName, byte priority, T message);
    Task InitializeDefinitionsAsync();
}