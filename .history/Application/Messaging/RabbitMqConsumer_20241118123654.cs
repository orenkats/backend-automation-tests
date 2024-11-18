using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

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

    public async Task StartConsumingAsync(string queueName, Func<string, Task> onMessageReceived)
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

            // Simulate processing the order 
            var order = JsonSerializer.Deserialize<OrderMessage>(message);
            if (order != null)
            {
                Console.WriteLine($"[x] Processing order: TraderId: {order.TraderId}, Stock: {order.StockSymbol}, Quantity: {order.Quantity}, Price: {order.Price}, Type: {order.OrderType}");
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

    private class OrderMessage
    {
        public Guid TraderId { get; set; }
        public string StockSymbol { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string OrderType { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
