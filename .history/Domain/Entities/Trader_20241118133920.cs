namespace Domain.Entities;

public class Trader
{
    public Guid Id { get; set; } // Primary key
    public string Name { get; set; } = null!;
    public decimal AccountBalance { get; set; }
    public ICollection<StockOrder> Orders { get; set; } = new List<StockOrder>();

    public void PlaceOrder(string stockSymbol, int quantity, decimal price, string orderType)
    {
        if (quantity <= 0 || price <= 0)
            throw new ArgumentException("Quantity and price must be positive.");

        var totalCost = quantity * price;
        if (orderType == "buy" && AccountBalance < totalCost)
            throw new InvalidOperationException("Insufficient funds.");

        // Update balance and create the order
        if (orderType == "buy")
            AccountBalance -= totalCost;

        Orders.Add(new StockOrder
        {
            Id = Guid.NewGuid(), // Unique identifier for the order
            TraderId = Id, // Assign the current Trader's Id
            StockSymbol = stockSymbol,
            Quantity = quantity,
            Price = price,
            OrderType = orderType,
            CreatedAt = DateTime.UtcNow
        });
    }
}
