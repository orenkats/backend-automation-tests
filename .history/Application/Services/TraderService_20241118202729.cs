using Application.Messaging;
using Application.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class TraderService
{
    private readonly AppDbContext _dbContext;
    private readonly RabbitMqPublisher _publisher;

    public TraderService(AppDbContext dbContext, RabbitMqPublisher publisher)
    {
        _dbContext = dbContext;
        _publisher = publisher;
    }

    // Place a new stock order for a trader
    public async Task PlaceOrderAsync(Guid traderId, string stockSymbol, int quantity, decimal price, string orderType)
    {
        // Fetch the trader from the database
        var trader = await _dbContext.Traders.FindAsync(traderId);
        if (trader == null)
        {
            throw new KeyNotFoundException("Trader not found.");
        }

        // Create and place the stock order
        trader.PlaceOrder(stockSymbol, quantity, price, orderType);

        
        // Update the database
        await _dbContext.SaveChangesAsync();

        // Publish the order to RabbitMQ
        var orderMessage = new
        {
            TraderId = traderId,
            StockSymbol = stockSymbol,
            Quantity = quantity,
            Price = price,
            OrderType = orderType,
            Timestamp = DateTime.UtcNow
        };

        await _publisher.PublishAsync("order_queue", orderMessage);

        Console.WriteLine("[x] Order placed and message published to RabbitMQ.");
    }

    // Add a new trader
    public async Task AddTraderAsync(Trader trader)
    {
        _dbContext.Traders.Add(trader);
        await _dbContext.SaveChangesAsync();
        Console.WriteLine("[x] Trader added to the database.");
    }

    // Get all traders
    public async Task<List<Trader>> GetAllTradersAsync()
    {
        return await _dbContext.Traders.ToListAsync();
    }

    // Get trader by ID
    public async Task<Trader?> GetTraderByIdAsync(Guid traderId)
    {
        return await _dbContext.Traders.FindAsync(traderId);
    }

    // Delete a trader by ID
    public async Task DeleteTraderAsync(Guid traderId)
    {
        var trader = await _dbContext.Traders.FindAsync(traderId);
        if (trader == null)
        {
            throw new KeyNotFoundException("Trader not found.");
        }

        _dbContext.Traders.Remove(trader);
        await _dbContext.SaveChangesAsync();
        Console.WriteLine("[x] Trader deleted from the database.");
    }
}
