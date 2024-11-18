using Application.Services;
using Application.Messaging;
using Application.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class TraderServiceTests
{
    private TraderService _service;
    private AppDbContext _dbContext;
    private RabbitMqPublisher _mockPublisher;

    public TraderServiceTests()
    {
        // Set up an in-memory database for testing
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;
        _dbContext = new AppDbContext(options);

        // Mock publisher for RabbitMQ
        _mockPublisher = new RabbitMqPublisher("localhost", "guest", "guest");

        // Initialize the TraderService
        _service = new TraderService(_dbContext, _mockPublisher);
    }

    [Fact]
  
    public async Task PlaceOrderAsync_ValidOrder_AddsOrderToDatabase()
    {
        // Arrange: Add a trader to the database
        var trader = new Trader
        {
            Id = Guid.NewGuid(),
            Name = "Test Trader",
            AccountBalance = 1000m
        };
        _dbContext.Traders.Add(trader);
        await _dbContext.SaveChangesAsync();

        // Act: Place an order
        await _service.PlaceOrderAsync(trader.Id, "AAPL", 10, 50m, "buy");

        // Assert: Validate the order was added to the database
        var savedOrder = await _dbContext.StockOrders
            .FirstOrDefaultAsync(o => o.TraderId == trader.Id);

        Assert.NotNull(savedOrder); // Order should exist
        Assert.Equal(trader.Id, savedOrder!.TraderId); // Ensure it's associated with the correct trader
        Assert.Equal("AAPL", savedOrder.StockSymbol); // Verify stock symbol
        Assert.Equal(10, savedOrder.Quantity); // Verify quantity
        Assert.Equal(50m, savedOrder.Price); // Verify price
        Assert.Equal("buy", savedOrder.OrderType); // Verify order type
    }

    [Fact]
    public async Task PlaceOrderAsync_InsufficientFunds_ThrowsException()
    {
        // Arrange
        var trader = new Trader
        {
            Id = Guid.NewGuid(),
            Name = "Test Trader",
            AccountBalance = 100m
        };
        _dbContext.Traders.Add(trader);
        await _dbContext.SaveChangesAsync();

        // Act & Assert: Expect an exception for insufficient funds
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.PlaceOrderAsync(trader.Id, "AAPL", 10, 50m, "buy"));
    }

    [Fact]
    public async Task PlaceOrderAsync_InvalidTraderId_ThrowsException()
    {
        // Act & Assert: Expect an exception for invalid trader ID
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.PlaceOrderAsync(Guid.NewGuid(), "AAPL", 10, 50m, "buy"));
    }
}
