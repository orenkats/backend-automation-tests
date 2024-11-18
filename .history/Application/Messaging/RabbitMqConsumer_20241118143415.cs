using Application.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Application.Messaging;

public class RabbitMqConsumer
{
    private readonly ConnectionFactory _factory;
    private readonly AppDbContext _dbContext;

    public RabbitMqConsumer(AppDbContext dbContext, string hostname = "localhost", string username = "guest", string password = "guest")
    {
        _factory = new ConnectionFactory
        {
            HostName = hostname,
            UserName = username,
            Password = password
        };
        _dbContext = dbContext;
    }

    public async Task StartConsumingAsync(string queueName, Func < string,Task> onMessageReceived)
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

            // Deserialize the order message
            var order = JsonSerializer.Deserialize<StockOrder>(message);
            if (order != null)
            {
                Console.WriteLine($"[x] Processing order: TraderId: {order.TraderId}, Stock: {order.StockSymbol}, Quantity: {order.Quantity}, Price: {order.Price}, Type: {order.OrderType}");

                // Save the order to the database
                await SaveOrderToDatabase(order);
            }

            // Acknowledge the message
            await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
        };

        await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
        Console.WriteLine("Press [Enter] to exit.");
        Console.ReadLine();
    }

    private async Task SaveOrderToDatabase(StockOrder order)
    {
        // Validate the trader exists
        var trader = await _dbContext.Traders.FindAsync(order.TraderId);
        if (trader == null)
        {
            Console.WriteLine($"[!] Trader with ID {order.TraderId} not found. Order discarded.");
            return;
        }

        // Save the order
        _dbContext.StockOrders.Add(order);
        await _dbContext.SaveChangesAsync();

        Console.WriteLine($"[x] Order saved to the database: {order.Id}");
    }
}
