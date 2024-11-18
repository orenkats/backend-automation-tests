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
    private readonly AppDbContext _dbContext;

    public RabbitMqConsumerTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("TestDb")); // Use an in-memory database
        services.AddSingleton<RabbitMqConsumer>();
        var serviceProvider = services.BuildServiceProvider();

        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        _consumer = new RabbitMqConsumer(scopeFactory);
        _dbContext = serviceProvider.GetRequiredService<AppDbContext>();
    }

    [Fact]
    public async Task SaveOrderToDatabase_SavesOrderForExistingTrader()
    {
        // Arrange: Add a trader to the database
        var traderId = Guid.NewGuid();
        var trader = new Trader
        {
            Id = traderId,
            Name = "Test Trader",
            AccountBalance = 1000m
        };
        _dbContext.Traders.Add(trader);
        await _dbContext.SaveChangesAsync();

        // Arrange: Create an order message
        var order = new StockOrder
        {
            Id = Guid.NewGuid(),
            TraderId = traderId,
            StockSymbol = "AAPL",
            Quantity = 5,
            Price = 100m,
            OrderType = "buy",
            CreatedAt = DateTime.UtcNow
        };

        // Act: Simulate message consumption
        await _consumer.SaveOrderToDatabase(order);

        // Assert: Verify the order is saved in the database
        var savedOrder = await _dbContext.StockOrders
            .FirstOrDefaultAsync(o => o.Id == order.Id);

        Assert.NotNull(savedOrder);
        Assert.Equal(order.TraderId, savedOrder!.TraderId);
        Assert.Equal(order.StockSymbol, savedOrder.StockSymbol);
        Assert.Equal(order.Quantity, savedOrder.Quantity);
        Assert.Equal(order.Price, savedOrder.Price);
        Assert.Equal(order.OrderType, savedOrder.OrderType);

        // Assert: Verify the trader's account balance is updated
        //var updatedTrader = await _dbContext.Traders
            //.FirstOrDefaultAsync(t => t.Id == traderId);

        //Assert.NotNull(updatedTrader);
        //Assert.Equal(500m, updatedTrader!.AccountBalance); // Ensure balance is updated
    }
}
