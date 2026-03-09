# MessageBrokerStrategies 

This project is designed to evaluate and compare the performance of three popular message broker solutions: **RabbitMQ**, **Kafka**, and **AWS SNS/SQS**. The goal is to provide insights into their throughput, latency, and reliability under various workloads, helping teams select the most suitable broker for their distributed systems.

## Project Purpose

- Benchmark and analyze the performance characteristics of RabbitMQ, Kafka, and AWS SNS/SQS.
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

### AWS SNS/SQS

**Advantages:**
- Fully managed, serverless messaging with high availability and durability.
- SNS provides pub/sub messaging, SQS provides queue-based decoupling and reliable delivery.
- Scales automatically, integrates natively with other AWS services.
- No infrastructure management required; pay-as-you-go pricing.
-- First million requests per month are free
- Suitable for cloud-native, distributed, and event-driven architectures.

**Protocol Used:**
- **HTTPS (SNS/SQS APIs), AWS SDKs**

---

## When to Prefer RabbitMQ, Kafka, or AWS SNS/SQS

**RabbitMQ** is ideal when you need:
- Flexible routing and message patterns (direct, topic, fanout, headers)
- Reliable transactional messaging and strong delivery guarantees
- Simple integration and management with a user-friendly UI
- Use cases like task queues, transactional workflows, or RPC

**Kafka** is preferred when you need:
- High throughput and scalability for large data streams
- Event sourcing, log aggregation, or real-time analytics
- Built-in message replay and long-term storage
- Distributed, fault-tolerant architecture for parallel processing

**AWS SNS/SQS** is suitable when you need:
- Fully managed messaging with minimal operational overhead
- Seamless integration with AWS services and automatic scaling
- Pub/sub and queue-based messaging in a serverless architecture
- High availability, durability, and security without managing infrastructure

Choose RabbitMQ for traditional messaging scenarios and complex routing. Choose Kafka for scalable event streaming and analytics. Choose AWS SNS/SQS for cloud-native, serverless applications with integrated AWS services.

---

## Feature Comparison: RabbitMQ vs Kafka vs AWS SNS/SQS (Pub/Sub)

| Feature                | RabbitMQ (Pub/Sub)                                     | Kafka (Pub/Sub)                                      | AWS SNS/SQS (Pub/Sub/Queue)                         |
|------------------------|--------------------------------------------------------|------------------------------------------------------|------------------------------------------------------|
| Protocol               | AMQP                                                   | Kafka TCP protocol                                   | HTTPS (SNS/SQS APIs), AWS SDKs                      |
| Message Durability     | Persistent queues, configurable per message/queue      | Always persistent, log-based storage                 | SQS: Always persistent, SNS: configurable            |
| Ordering Guarantees    | Per queue, can be affected by consumer concurrency     | Per partition, strict ordering within partition      | SQS: FIFO queues for strict ordering, SNS: best effort |
| Scalability            | Scales with exchanges/queues, clustering supported     | Scales with partitions, distributed by design        | Fully managed, auto-scaling, unlimited throughput    |
| Consumer Model         | Push-based, consumers receive messages as delivered    | Pull-based, consumers fetch messages on demand       | SQS: Poll-based, SNS: Push-based to endpoints/queues |
| Replay/Recovery        | Limited, requires dead-letter queue or manual handling | Built-in, consumers can replay from any offset       | SQS: Dead-letter queues, SNS: no replay              |
| Message Acknowledgment | Manual or automatic, supports retries                  | Offset commit, manual or automatic                   | SQS: Automatic, visibility timeout, retries          |
| Routing Flexibility    | Exchanges (direct, topic, fanout, headers)             | Topics only, no built-in routing                     | SNS: Topic-based pub/sub, SQS: queue-based          |
| Management UI          | Web-based management interface                         | No official UI, third-party tools (e.g., Confluent)  | AWS Console, CLI, SDKs                              |
| Transaction Atomicity  | Yes, supports transactions                             | Limited, idempotency via producer/consumer configs   | SQS: Limited, FIFO queues support exactly-once       |
| Delivery Guarantees    | At-most-once, at-least-once, exactly-once (with tx)    | At-most-once, at-least-once, exactly-once (with tx) | SQS: At-least-once, FIFO: exactly-once, SNS: best effort |
| Consumer Groups        | Not native, can be emulated with queues                | Native, enables parallel processing                  | SQS: Not native, can be emulated with multiple queues|
| Message TTL            | Supported                                              | Not natively supported                               | SQS: Supported, SNS: Not supported                   |
| Use Cases              | Task queues, transactional messaging, RPC              | Event streaming, log aggregation, analytics          | Cloud-native, serverless, decoupling, fanout, integration |

---

## RabbitMQ Usage Guide

### Prerequisites 

RabbitMQ must be installed and running. Environment is Development
- URL: http://127.0.0.1:15672/
- User: guest
- Password: guest

**RabbitMQ Installation:**
- Download and install RabbitMQ from [https://www.rabbitmq.com/download.html](https://www.rabbitmq.com/download.html)

To use RabbitMQ in this project:

1. **Configure Exchanges in RabbitMessageBroker `appsettings.json`.**
   - Define exchanges in Rabbit.Exchanges.Name.

2. **Start RabbitMessageBroker first.**
   - This service must be running before any consumers connect.

3. **Configure Queues in RabbitMessageConsumer `appsettings.json`.**
   - Define queues in Rabbit.Exchanges.Queue.Name.

4. **Run RabbitMessageConsumer.**
   - The consumer binds the queue to an exchange, if the queue doesn't exist the queue is created.

5. **Test publishing messages to an exchange:**
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
   - When RabbitMessageConsumer is started, messages with higher priority are received first.

---

## Kafka Usage Guide

### Prerequisites

Kafka is a product that runs on the Java Virtual Machine (JVM). Before using Kafka in this project, start a Kafka broker outside the .NET solution.

**Kafka Installation:**
- Download and install Apache Kafka from [https://kafka.apache.org/downloads](https://kafka.apache.org/downloads)

**To start Kafka on Windows:**
```
C:\kafka_2.13-4.2.0\bin\windows\kafka-server-start.bat C:\kafka_2.13-4.2.0\config\server.properties
```
Ensure Java is installed and the Kafka distribution extracted to the specified path.

---

### Setting Topics and Partitions

Kafka organises messages into **topics**, which are further divided into **partitions**. Topics allow categorise messages, while partitions enable parallelism and scalability. More partitions allow higher throughput and distributed processing, but require careful management of ordering and consumer load.

**In the KafkaMessageBroker app:**
- The topic name and partition count are configured in `appsettings.json`.
- Setting the topic and partition count is crucial for performance and scalability. Each partition can be processed independently, allowing multiple consumers to read from the same topic in parallel.

---

### Subscribing to a Topic in KafkaMessageConsumer

The `KafkaMessageConsumer` subscribes to a topic specified in its configuration. Subscription is handled in  `appsettings.json`.

**Kafka Topic Options:**
- **AutoOffsetReset**: Determines where the consumer starts reading if no offset is present.  
  - `Earliest`: Reads all available messages from the beginning.
  - `Latest`: Reads only new messages arriving after subscription.
- **GroupId**: Consumers with the same group ID share the workload for a topic (load balancing). Each message is delivered to one consumer in the group. Changing the group ID will cause the consumer to start from the offset defined by `AutoOffsetReset`.
- **Topic**: Must match the topic created in KafkaMessageBroker and is set in `appsettings.json`.
- **EnableAutoCommit** is true, offsets are committed automatically, which can affect message replay and recovery.
- Ensure the topic exists and matches the configuration, otherwise the consumer will fail to subscribe.

---

### Example Workflow

1. **Start Kafka broker**.
2. **Configure topic and partitions** in KafkaMessageBroker `appsettings.json`.
3. **Run KafkaMessageBroker** to create the topic.
4. **Configure topic and group ID** in KafkaMessageConsumer `appsettings.json`.
5. **Run KafkaMessageConsumer** to subscribe and process messages.
6. **Test publishing messages to an exchange:**
   - Example endpoint:
   - `https://localhost:7065/kafka/publish/topic/test-topic?key=order-101`
   - Example payload:
     ```json
     {
      "orderId": "ORD-101",
      "customer": "Galej Test",
      "items": ["Kafka Guide", "C# Reference"],
      "total": 45.99,
      "status": "Completed"
     }
     ```
---

## AWS Usage Guide

### Prerequisites

AWS SNS and SQS must be available in your AWS account.  
- AWS credentials (Access Key ID and Secret Access Key) must be configured for local development.
- Region: Specify your AWS region (e.g., `ap-southeast-4`) in configuration.

**AWS Setup:**
- Create an SNS topic in the AWS Console or programmatically.
- Create an SQS queue and subscribe it to the SNS topic.
- Ensure your IAM user has permissions for SNS and SQS actions.

**Local Development:**
- Install AWS CLI and configure credentials:  
```
  aws configure
```
- Optionally, use [LocalStack](https://github.com/localstack/localstack) for local AWS service emulation.

---

### Configuring SNS Topics and SQS Queues

**In the AwsMessageBroker app:**
- SNS topic name is configured in `appsettings.json`.
- The SNS topic manager creates topics if it does not exist.

---

### Consuming Messages from SQS

The `SqsConsumerService` polls the configured SQS queue for messages.

**Queue configuration:**
- Set the queue name in AwsMessageConsumer `appsettings.json`.
- The consumer automatically processes messages as they arrive.

**Example workflow:**

1. **Configure topic** in AwsMessageBroker `appsettings.json`.
2. **Run AwsMessageBroker** to ensure topic exists.
3. **Configure queue name** in AwsMessageConsumer `appsettings.json`.
4. **Run AwsMessageConsumer** to start polling and processing messages.
5. **Test publishing messages to SNS topic:**
 - Example endpoint:
   - `https://localhost:7117/aws/publish`
 - Example payload:
   ```json
   {
       "message": "This message is from Postman",
       "subject": "PerformanceTest"
     }
     ```
---


