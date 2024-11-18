using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Domain.Entities;

namespace Application.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<Trader> Traders { get; set; }
    public DbSet<StockOrder> StockOrders { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
