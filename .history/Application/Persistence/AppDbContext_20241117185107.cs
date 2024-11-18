using Microsoft.EntityFrameworkCore; // For DbContext and DbSet
using Domain.Entities; // For Trader and StockOrder

namespace Application.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<Trader> Traders { get; set; } = null!;
    public DbSet<StockOrder> StockOrders { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
