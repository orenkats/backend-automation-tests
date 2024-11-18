using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

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

        Console.WriteLine($"Queue declared: {queueName}");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine($"[x] Received: {message}");

            try
            {
                await onMessageReceived(message);
                await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                Console.WriteLine($"Message acknowledged: {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }

                // Acknowledge message
                await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            };

        await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
        Console.WriteLine($"Consumer started for queue: {queueName}");
    }
}
