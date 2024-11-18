using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Domain.Entities; // Reference for StockOrder

namespace Application.Messaging;

public class RabbitMqConsumer
{
    private readonly ConnectionFactory _factory;

    public RabbitMqConsumer(string hostname = "localhost", string username = "guest", string password = "guest")
    {
        _factory = new ConnectionFactory
        {
            HostName = hostname,
            UserName = username,
            Password = password
        };
    }

    public async Task StartConsumingAsync(string queueName,Func<string, Task> onMessageReceived)
    {
        using var connection = await _factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        Console.WriteLine($"[*] Waiting for messages in {queueName}...");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine($"[x] Received message: {message}");

            // Deserialize the message into StockOrder
            var stockOrder = JsonSerializer.Deserialize<StockOrder>(message);
            if (stockOrder != null)
            {
                Console.WriteLine($"[x] Processing order: TraderId: {stockOrder.Id}, Stock: {stockOrder.StockSymbol}, Quantity: {stockOrder.Quantity}, Price: {stockOrder.Price}, Type: {stockOrder.OrderType}");
                // Simulate a delay to process the order
                await Task.Delay(1000);
                Console.WriteLine("[x] Order processed.");
            }

            // Acknowledge the message
            await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
        };

        await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
        Console.WriteLine("Press [Enter] to exit.");
        Console.ReadLine();
    }
}
