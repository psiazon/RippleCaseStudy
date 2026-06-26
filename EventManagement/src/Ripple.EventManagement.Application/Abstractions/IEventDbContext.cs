using Microsoft.EntityFrameworkCore;
using Ripple.EventManagement.Domain.Entities;

namespace Ripple.EventManagement.Application.Abstractions;

public interface IEventDbContext
{
    DbSet<Event> Events { get; }
    DbSet<PricingTier> PricingTiers { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
