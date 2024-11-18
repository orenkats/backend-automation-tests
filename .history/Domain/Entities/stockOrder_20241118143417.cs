namespace Domain.Entities;

public class StockOrder
{
    public Guid Id { get; set; } // Primary key
    public Guid TraderId { get; set; } // Foreign key linking to Trader
    public string StockSymbol { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string OrderType { get; set; } = null!; // e.g., "buy" or "sell"
    public DateTime CreatedAt { get; set; }
}
