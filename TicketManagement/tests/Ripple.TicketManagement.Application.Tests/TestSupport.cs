using Microsoft.EntityFrameworkCore;
using Ripple.TicketManagement.Application.Abstractions;
using Ripple.TicketManagement.Domain.Entities;

namespace Ripple.TicketManagement.Application.Tests;

internal sealed class TestTicketDbContext(DbContextOptions<TestTicketDbContext> options) : DbContext(options), ITicketDbContext
{
    public DbSet<TicketInventory> Inventories => Set<TicketInventory>();
    public DbSet<TicketOrder> Orders => Set<TicketOrder>();
}

internal static class TestDbContextFactory
{
    public static TestTicketDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TestTicketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestTicketDbContext(options);
    }
}

internal sealed class StubEventCatalogClient(EventCatalogItem? item) : IEventCatalogClient
{
    public Task<EventCatalogItem?> GetEventAsync(Guid eventId, CancellationToken cancellationToken) => Task.FromResult(item);
}
