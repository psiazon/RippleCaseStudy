using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Ripple.TicketManagement.Domain.Entities;
using Ripple.TicketManagement.Infrastructure.Persistence;
using Xunit;

namespace Ripple.TicketManagement.Infrastructure.Tests;

public sealed class TicketDbContextTests
{
    [Fact]
    public async Task TicketDbContext_ShouldPersistInventoriesAndOrders()
    {
        var options = new DbContextOptionsBuilder<TicketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new TicketDbContext(options);
        var eventId = Guid.NewGuid();
        var inventory = new TicketInventory(eventId, 20);
        inventory.Purchase(5);
        db.Inventories.Add(inventory);
        db.Orders.Add(new TicketOrder(eventId, "buyer@example.com", 2, 7.5m));

        await db.SaveChangesAsync();

        (await db.Inventories.SingleAsync()).AvailableQuantity.Should().Be(15);
        (await db.Orders.SingleAsync()).TotalPrice.Should().Be(15m);
    }

    [Fact]
    public void TicketDbContext_Model_ShouldConfigureExpectedTablesAndProperties()
    {
        var options = new DbContextOptionsBuilder<TicketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var db = new TicketDbContext(options);

        var inventory = db.Model.FindEntityType(typeof(TicketInventory));
        var order = db.Model.FindEntityType(typeof(TicketOrder));

        inventory!.GetTableName().Should().Be("TicketInventories");
        inventory.FindPrimaryKey()!.Properties.Single().Name.Should().Be(nameof(TicketInventory.EventId));
        inventory.FindProperty(nameof(TicketInventory.RowVersion))!.IsConcurrencyToken.Should().BeTrue();
        order!.GetTableName().Should().Be("TicketOrders");
        order.FindPrimaryKey()!.Properties.Single().Name.Should().Be(nameof(TicketOrder.Id));
        order.FindProperty(nameof(TicketOrder.PurchaserEmail))!.GetMaxLength().Should().Be(320);        
    }
}
