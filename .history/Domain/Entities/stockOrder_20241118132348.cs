namespace Domain.Entities;

public class StockOrder
{
    public Guid Id { get; set; }
    public Guid TraderId { get; set; } // Added this line
    public string StockSymbol { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string OrderType { get; set; } // e.g., "buy" or "sell"
    public DateTime CreatedAt { get; set; }
}
