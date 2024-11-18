namespace Domain.Entities;

public class StockOrder
{
    public Guid StockOrderId { get; set; }
    public string StockSymbol { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string OrderType { get; set; }
    public DateTime CreatedAt { get; set; }
}
