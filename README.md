# MessageBrokerStrategies Performance Testing

This project is designed to evaluate and compare the performance of two popular message broker solutions: **RabbitMQ** and **Kafka**. The goal is to provide insights into their throughput, latency, and reliability under various workloads, helping teams select the most suitable broker for their distributed systems.

## Project Purpose

- Benchmark and analyze the performance characteristics of RabbitMQ and Kafka.
- Demonstrate integration strategies for each broker within .NET 9 applications.
- Highlight the strengths and trade-offs of each solution.

## Message Broker Solutions

### RabbitMQ

**Advantages:**
- Implements the Advanced Message Queuing Protocol (AMQP), supporting reliable messaging and flexible routing.
- Easy to set up and manage, with a user-friendly management interface.
- Strong support for message acknowledgments, delivery guarantees, and complex routing patterns (topics, direct, fanout).
- Suitable for scenarios requiring transactional messaging and interoperability.

**Protocol Used:**  
- **AMQP (Advanced Message Queuing Protocol)**

### Kafka

**Advantages:**
- High throughput and scalability, designed for handling large volumes of data streams.
- Distributed, fault-tolerant architecture with built-in replication and partitioning.
- Excellent for event sourcing, real-time analytics, and log aggregation.
- Guarantees message durability and supports replaying messages.

**Protocol Used:**  
- **Kafka’s native protocol (TCP-based, optimized for streaming and partitioned data)**

---

## RabbitMQ Usage Guide

### Prerequisites 

Rabbit is running. Environment is Development
- URL: http://127.0.0.1:15672/
- User: guest
- Password: guest

To use RabbitMQ in this project:

1. **Start RabbitMessageBroker first.**
   - This service must be running before any consumers connect.

2. **Configure exchanges and queues in `appsettings.json`.**
   - Define your exchanges and queues in the configuration file for RabbitMessageBroker.

3. **Run RabbitMessageConsumer.**
   - The consumer connects to a queue. Ensure the queue exists in RabbitMessageBroker.

4. **Test publishing messages to an exchange:**
   - Example endpoint:
     - `https://localhost:7277/rabbit/publish/exchange/Test-Exchange-1`
   - Example payload:
     ```json
     {
       "message": "This message is from Postman",
       "priority": 10
     }
     ```

5. **Message Priority Behavior:**
   - Try publishing messages while RabbitMessageConsumer is down.
   - - Example payload 1:
     ```json
     {
       "message": "This message is from Postman",
       "priority": 5
     }
     ```
   - - Example payload 2:
     ```json
     {
       "message": "This message is from Postman",
       "priority": 10
     }
     ```
   - When RabbitMessageConsumer is started, notice that messages with higher priority are received first.

---
