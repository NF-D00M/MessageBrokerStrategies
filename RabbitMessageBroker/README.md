# RabbitMessageBroker

Url: http://127.0.0.1:15672/
Username: guest
Password: guest

# RabbitMessageBroker

This project demonstrates how to integrate RabbitMQ as a message broker within a .NET 9 application. It covers installation, configuration of exchanges and queues, and testing message publishing via a dedicated endpoint.

## Configuring Exchanges and Queues

To add exchanges and queues, update your application configuration (e.g., `appsettings.json`):
```json { "RabbitMQ": { "Host": "localhost", "Username": "guest", "Password": "guest", "Exchange": "test-exchange", "Queue": "test-queue", "RoutingKey": "test-routing-key" } } ```

## Testing Message Publishing

A publish endpoint is provided to test sending messages to RabbitMQ.

**Example HTTP Request:**

POST https://localhost:7277/rabbit/publish/exchange/Test-Exchange-1 
Content-Type: application/json
{ "message": "Hello, RabbitMQ!" }

This will publish the message to the configured exchange and queue(s). You can verify delivery using the RabbitMQ Management UI.

---

## Message Delivery Types in RabbitMQ

RabbitMQ supports multiple exchange types for message delivery, with **direct** and **fanout** being two common strategies:

### 1. Direct Exchange

- **How it works:**  
  A direct exchange routes messages to queues based on a specific routing key. Each message is delivered only to the queue(s) whose binding key exactly matches the routing key of the message.
- **Implementation detail:**  
  When publishing to a direct exchange, the application must loop through each queue and send the message individually if multiple queues need to receive the same message. This can introduce overhead, especially as the number of queues increases.

### 2. Fanout Exchange

- **How it works:**  
  A fanout exchange routes messages to all queues bound to it, regardless of routing keys. Every queue receives a copy of each message published to the exchange.
- **Performance advantage:**  
  Fanout exchanges are more performant because RabbitMQ handles the distribution internally, broadcasting the message to all bound queues in a single operation. There is no need for the application to loop through queues or manage routing keys, resulting in lower latency and higher throughput when broadcasting messages.

**Summary:**  
Use direct exchanges for targeted delivery to specific queues, but expect additional overhead when multiple queues are involved. Use fanout exchanges for efficient broadcasting to all queues, benefiting from RabbitMQ’s optimized internal message distribution.