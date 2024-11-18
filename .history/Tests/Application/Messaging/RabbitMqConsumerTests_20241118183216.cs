using Application.Messaging;
using Application.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

public class RabbitMqConsumerTests
{
    private readonly RabbitMqConsumer _consumer;
    private readonly RabbitMqPublisher _publisher;
    private readonly AppDbContext _dbContext;

    public RabbitMqConsumerTests()
    {
        // Set up dependency injection
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("TestDb")); // Use an in-memory database
        services.AddSingleton<RabbitMqPublisher>();
        services.AddSingleton<IServiceScopeFactory>(provider =>
            provider.GetRequiredService<IServiceScopeFactory>());
        var serviceProvider = services.BuildServiceProvider();

        _dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        _consumer = new RabbitMqConsumer(scopeFactory);
        _publisher = new RabbitMqPublisher("localhost", "guest", "guest");
    }

    [Fact]
    public async Task Consumer_SavesOrderToDatabase_UpdatesTraderBalance()
    {
        // Arrange
        var trader = new Trader
        {
            Id = Guid.NewGuid(),
            Name = "Test Trader",
            AccountBalance = 1000m
        };

        _dbContext.Traders.Add(trader);
        await _dbContext.SaveChangesAsync();

        var orderMessage = new StockOrder
        {
            Id = Guid.NewGuid(),
            TraderId = trader.Id,
            StockSymbol = "AAPL",
            Quantity = 5,
            Price = 100m,
            OrderType = "buy",
            CreatedAt = DateTime.UtcNow
        };

        var serializedMessage = JsonSerializer.Serialize(orderMessage);

        // Act
        var consumingTask = _consumer.StartConsumingAsync("order_queue", async (msg) =>
        {
            Console.WriteLine($"[TEST] Consumed message: {msg}");
            await Task.CompletedTask;
        });

        await _publisher.PublishAsync("order_queue", serializedMessage);

        // Allow time for the message to be processed
        await Task.Delay(1000);

        // Assert
        var savedOrder = await _dbContext.StockOrders.FirstOrDefaultAsync(o => o.Id == orderMessage.Id);
        var updatedTrader = await _dbContext.Traders.FirstOrDefaultAsync(t => t.Id == trader.Id);

        Assert.NotNull(savedOrder); // Order should exist in the database
        Assert.NotNull(updatedTrader); // Trader should exist in the database
        Assert.Equal(trader.Id, savedOrder.TraderId); // Order should belong to the correct trader
        Assert.Equal(500m, updatedTrader!.AccountBalance); // Trader's account balance should be updated correctly
    }
}
