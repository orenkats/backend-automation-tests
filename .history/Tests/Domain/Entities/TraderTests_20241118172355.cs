using Domain.Entities;
using System;
using Xunit;

public class TraderTests
{
    [Fact]
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
        Assert.Equal(500m, trader.AccountBalance); // Verify account balance is updated correctly
        Assert.Single(trader.Orders); // Ensure one order is added
    }

    [Fact]
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
