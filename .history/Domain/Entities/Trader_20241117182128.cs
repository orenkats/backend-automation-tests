namespace Domain.Entities;

public class Trader
{
    public Guid TraderId { get; set; }
    public string Name { get; set; }
    public List<StockOrder> Orders { get; set; } = new List<StockOrder>();

    public void PlaceOrder(string stockSymbol, int quantity, decimal price, string orderType)
    {
        var order = new StockOrder
        {
            StockOrderId = Guid.NewGuid(),
            StockSymbol = stockSymbol,
            Quantity = quantity,
            Price = price,
            OrderType = orderType,
            CreatedAt = DateTime.UtcNow
        };

        Orders.Add(order);
    }
}
