using Application.Services;
using Application.Messaging;
using Application.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

public class TraderServiceTests
{
    private readonly TraderService _service;
    private readonly AppDbContext _dbContext;
    private readonly RabbitMqPublisher _mockPublisher;

    public TraderServiceTests()
    {
        // Use an in-memory database for testing
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;
        _dbContext = new AppDbContext(options);
        _mockPublisher = new RabbitMqPublisher("localhost", "guest", "guest");

        // Initialize the TraderService with dependencies
        _service = new TraderService(_dbContext, _mockPublisher);
    }

    [Fact]
    public async Task PlaceOrderAsync_PublishesMessageAndUpdatesDatabase()
    {
        // Arrange
        var trader = new Trader
        {
            Id = Guid.NewGuid(),
            Name = "Test Trader",
            AccountBalance = 1000m
        };

        // Add and save the trader to the in-memory database
        _dbContext.Traders.Add(trader);
        await _dbContext.SaveChangesAsync();

        // Act
        await _service.PlaceOrderAsync(trader.Id, "AAPL", 10, 50m, "buy");

        // Assert
        var updatedTrader = await _dbContext.Traders
            .Include(t => t.Orders) // Include Orders to ensure the collection is loaded
            .FirstOrDefaultAsync(t => t.Id == trader.Id);

        // Verify that the trader's account balance has been updated
        Assert.NotNull(updatedTrader);
        Assert.Equal(500m, updatedTrader!.AccountBalance); // Updated balance
        Assert.Single(updatedTrader.Orders); // One order added

        // Verify the order details
        var order = updatedTrader.Orders.First();
        Assert.Equal("AAPL", order.StockSymbol);
        Assert.Equal(10, order.Quantity);
        Assert.Equal(50m, order.Price);
        Assert.Equal("buy", order.OrderType);
    }
}