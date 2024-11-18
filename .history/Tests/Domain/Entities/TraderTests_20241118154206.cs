using Domain.Entities;
using NUnit.Framework;
using System;

[TestFixture]
public class TraderTests
{
    [Test]
    public void PlaceOrder_DecreasesAccountBalance_OnBuyOrder()
    {
        // Arrange
        var trader = new Trader
        {
            Id = Guid.NewGuid(),
            Name = "Test Trader",
            AccountBalance = 1000m
        };

        // Act
        trader.PlaceOrder("AAPL", 10, 50m, "buy");

        // Assert
        Assert.AreEqual(500m, trader.AccountBalance);
        Assert.AreEqual(1, trader.Orders.Count);
    }

    [Test]
    public void PlaceOrder_ThrowsException_WhenInsufficientFunds()
    {
        // Arrange
        var trader = new Trader
        {
            Id = Guid.NewGuid(),
            Name = "Test Trader",
            AccountBalance = 100m
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            trader.PlaceOrder("AAPL", 10, 50m, "buy");
        });
    }
}
