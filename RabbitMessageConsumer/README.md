# RabbitMessageConsumer

## Purpose

This project is designed to test binding a service to a RabbitMQ exchange by allowing the consumer to create its own queue. The implementation requires that the exchange already exists prior to running the consumer. Additionally, each queue name must be unique to prevent load balancing messages between multiple services using the same queue.

## Usage Instructions

1. **Start the RabbitMessageBroker service.**
2. **Start the RabbitMessageConsumer service.**
3. Use the publish endpoint to send messages. https://localhost:7277/rabbit/publish/exchange/Test-Exchange-1
4. Observe messages being consumed by both the RabbitMessageBroker application and the RabbitMessageConsumer application.

Ensure that the queue name configured for the consumer is unique and that the target exchange exists before starting the consumer.