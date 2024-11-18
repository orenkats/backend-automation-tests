using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Application.Messaging;

public class RabbitMqPublisher
{
    private readonly ConnectionFactory _factory;

    public RabbitMqPublisher(string hostname = "localhost", string username = "guest", string password = "guest")
    {
        _factory = new ConnectionFactory
        {
            HostName = hostname,
            UserName = username,
            Password = password
        };
    }

    public async Task PublishAsync(string queueName, object orderMessage)
    {
        using var connection = await _factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        // Serialize the message to JSON
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(orderMessage));

        // Publish the message
        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: queueName,
            mandatory: false,
            body: body);

        Console.WriteLine($"[x] Message published to queue: {queueName}");
    }
}
