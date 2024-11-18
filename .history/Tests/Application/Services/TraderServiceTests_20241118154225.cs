using Application.Services;
using Application.Messaging;
using Application.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

[TestFixture]
public class TraderServiceTests
{
    private TraderService _service;
    private AppDbContext _dbContext;
    private RabbitMqPublisher _mockPublisher;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;
        _dbContext = new AppDbContext(options);
        _mockPublisher = new RabbitMqPublisher("localhost", "guest", "guest");
        _service = new TraderService(_dbContext, _mockPublisher);
    }

    [Test]
    public async Task PlaceOrderAsync_PublishesMessageAndUpdatesDatabase()
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

        // Act
        await _service.PlaceOrderAsync(trader.Id, "AAPL", 10, 50m, "buy");

        // Assert
        var updatedTrader = await _dbContext.Traders.FindAsync(trader.Id);
        Assert.AreEqual(500m, updatedTrader!.AccountBalance);
        Assert.AreEqual(1, updatedTrader.Orders.Count);
    }
}
