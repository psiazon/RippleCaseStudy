using Microsoft.EntityFrameworkCore;
using Ripple.TicketManagement.Application.Abstractions;
using Ripple.TicketManagement.Domain.Entities;

namespace Ripple.TicketManagement.Infrastructure.Persistence;

public sealed class TicketDbContext(DbContextOptions<TicketDbContext> options) : DbContext(options), ITicketDbContext
{
    public DbSet<TicketInventory> Inventories => Set<TicketInventory>();
    public DbSet<TicketOrder> Orders => Set<TicketOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TicketInventory>(b =>
        {
            b.ToTable("TicketInventories");
            b.HasKey(x => x.EventId);
            b.Property(x => x.RowVersion).IsRowVersion();
        });
        modelBuilder.Entity<TicketOrder>(b =>
        {
            b.ToTable("TicketOrders");
            b.HasKey(x => x.Id);
            b.Property(x => x.PurchaserEmail).HasMaxLength(320).IsRequired();
            b.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        });
    }
}
