using Microsoft.EntityFrameworkCore;
using Ripple.TicketManagement.Domain.Entities;

namespace Ripple.TicketManagement.Application.Abstractions;

public interface ITicketDbContext
{
    DbSet<TicketInventory> Inventories { get; }
    DbSet<TicketOrder> Orders { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
