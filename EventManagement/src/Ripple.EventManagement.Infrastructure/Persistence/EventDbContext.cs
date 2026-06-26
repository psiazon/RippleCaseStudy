using Microsoft.EntityFrameworkCore;
using Ripple.EventManagement.Application.Abstractions;
using Ripple.EventManagement.Domain.Entities;

namespace Ripple.EventManagement.Infrastructure.Persistence;

public sealed class EventDbContext(DbContextOptions<EventDbContext> options) : DbContext(options), IEventDbContext
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<PricingTier> PricingTiers => Set<PricingTier>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(b =>
        {
            b.ToTable("Events");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.Description).HasMaxLength(2000);
            b.Property(x => x.Venue).HasMaxLength(300).IsRequired();
            b.HasMany(x => x.PricingTiers).WithOne().HasForeignKey("EventId").OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<PricingTier>(b =>
        {
            b.ToTable("PricingTiers");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(100).IsRequired();
            b.Property(x => x.Price).HasColumnType("decimal(18,2)");
        });
    }
}
