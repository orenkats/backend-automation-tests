using Application.Persistence;
using Application.Messaging;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class SaveOrderToDatabaseTests
{
    private AppDbContext _dbContext;
    private RabbitMqConsumer _consumer;

    public SaveOrderToDatabaseTests()
    {
        // Setup in-memory database
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));

        var serviceProvider = services.BuildServiceProvider();
        _dbContext = serviceProvider.GetRequiredService<AppDbContext>();

        // Initialize RabbitMqConsumer
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        _consumer = new RabbitMqConsumer(scopeFactory);
    }

    [Fact]
    public async Task SaveOrderToDatabase_ValidOrder_SavesSuccessfully()
    {
        // Arrange
        var traderId = Guid.NewGuid();
        var trader = new Trader
        {
            Id = traderId,
            Name = "Test Trader",
            AccountBalance = 1000m
        };

        // Add trader to the database
        _dbContext.Traders.Add(trader);
        await _dbContext.SaveChangesAsync();

        var order = new StockOrder
        {
            Id = Guid.NewGuid(),
            TraderId = traderId,
            StockSymbol = "AAPL",
            Quantity = 10,
            Price = 100m,
            OrderType = "buy",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _consumer.SaveOrderToDatabase(order);

        // Assert
        var savedOrder = await _dbContext.StockOrders.FindAsync(order.Id);
        Assert.NotNull(savedOrder); // Order should be saved
        Assert.Equal(order.TraderId, savedOrder.TraderId);
        Assert.Equal(order.StockSymbol, savedOrder.StockSymbol);
        Assert.Equal(order.Quantity, savedOrder.Quantity);
        Assert.Equal(order.Price, savedOrder.Price);
        Assert.Equal(order.OrderType, savedOrder.OrderType);
    }

    [Fact]
    public async Task SaveOrderToDatabase_InvalidTrader_Discarded()
    {
        // Arrange
        var invalidTraderId = Guid.NewGuid();
        var order = new StockOrder
        {
            Id = Guid.NewGuid(),
            TraderId = invalidTraderId,
            StockSymbol = "AAPL",
            Quantity = 10,
            Price = 100m,
            OrderType = "buy",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _consumer.SaveOrderToDatabase(order);

        // Assert
        var savedOrder = await _dbContext.StockOrders.FindAsync(order.Id);
        Assert.Null(savedOrder); // Order should not be saved
    }
}
