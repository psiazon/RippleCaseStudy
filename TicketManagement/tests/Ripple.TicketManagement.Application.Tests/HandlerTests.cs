using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ripple.TicketManagement.Application.Abstractions;
using Ripple.TicketManagement.Application.Tickets;
using Ripple.TicketManagement.Domain.Entities;
using Xunit;

namespace Ripple.TicketManagement.Application.Tests;

public sealed class HandlerTests
{
    [Fact]
    public async Task CreateInventoryHandler_ShouldCreateInventory()
    {
        await using var db = TestDbContextFactory.Create();
        var command = new CreateInventoryCommand(Guid.NewGuid(), 100);

        var result = await new CreateInventoryHandler(db).Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
        var inventory = await db.Inventories.SingleAsync();
        inventory.EventId.Should().Be(command.EventId);
        inventory.TotalCapacity.Should().Be(100);
    }

    [Fact]
    public async Task CreateInventoryHandler_ShouldRejectDuplicateInventory()
    {
        await using var db = TestDbContextFactory.Create();
        var eventId = Guid.NewGuid();
        db.Inventories.Add(new TicketInventory(eventId, 10));
        await db.SaveChangesAsync();

        var act = () => new CreateInventoryHandler(db).Handle(new CreateInventoryCommand(eventId, 20), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("An inventory record for the specified event already exists.");
    }

    [Fact]
    public async Task GetAvailabilityHandler_ShouldReturnExistingInventory()
    {
        await using var db = TestDbContextFactory.Create();
        var eventId = Guid.NewGuid();
        var inventory = new TicketInventory(eventId, 10);
        inventory.Purchase(4);
        db.Inventories.Add(inventory);
        await db.SaveChangesAsync();
        var eventItem = new EventCatalogItem(eventId, "Concert", 10, Array.Empty<EventPriceTier>());

        var dto = await new GetAvailabilityHandler(db, new StubEventCatalogClient(eventItem)).Handle(new GetAvailabilityQuery(eventId), CancellationToken.None);

        dto.Should().Be(new AvailabilityDto(eventId, 10, 4, 6));
    }

    [Fact]
    public async Task GetAvailabilityHandler_ShouldUseEventCapacity_WhenInventoryDoesNotExist()
    {
        await using var db = TestDbContextFactory.Create();
        var eventId = Guid.NewGuid();
        var eventItem = new EventCatalogItem(eventId, "Concert", 50, Array.Empty<EventPriceTier>());

        var dto = await new GetAvailabilityHandler(db, new StubEventCatalogClient(eventItem)).Handle(new GetAvailabilityQuery(eventId), CancellationToken.None);

        dto.Should().Be(new AvailabilityDto(eventId, 50, 0, 50));
    }

    [Fact]
    public async Task GetAvailabilityHandler_ShouldThrow_WhenEventDoesNotExist()
    {
        await using var db = TestDbContextFactory.Create();

        var act = () => new GetAvailabilityHandler(db, new StubEventCatalogClient(null)).Handle(new GetAvailabilityQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Event not found.");
    }

    [Fact]
    public async Task PurchaseTicketsHandler_ShouldCreateInventoryAndOrder()
    {
        await using var db = TestDbContextFactory.Create();
        var eventId = Guid.NewGuid();
        var tierId = Guid.NewGuid();
        var evt = new EventCatalogItem(eventId, "Game", 25, new[] { new EventPriceTier(tierId, "VIP", 15m) });
        var command = new PurchaseTicketsCommand(eventId, tierId, " Buyer@Example.com ", 3);

        var dto = await new PurchaseTicketsHandler(db, new StubEventCatalogClient(evt)).Handle(command, CancellationToken.None);

        dto.EventId.Should().Be(eventId);
        dto.PurchaserEmail.Should().Be("buyer@example.com");
        dto.Quantity.Should().Be(3);
        dto.UnitPrice.Should().Be(15m);
        dto.TotalPrice.Should().Be(45m);
        (await db.Inventories.SingleAsync()).SoldQuantity.Should().Be(3);
        (await db.Orders.SingleAsync()).TotalPrice.Should().Be(45m);
    }

    [Fact]
    public async Task PurchaseTicketsHandler_ShouldUseExistingInventoryAndPreventOverselling()
    {
        await using var db = TestDbContextFactory.Create();
        var eventId = Guid.NewGuid();
        var tierId = Guid.NewGuid();
        var inventory = new TicketInventory(eventId, 2);
        db.Inventories.Add(inventory);
        await db.SaveChangesAsync();
        var evt = new EventCatalogItem(eventId, "Game", 2, new[] { new EventPriceTier(tierId, "GA", 10m) });

        var act = () => new PurchaseTicketsHandler(db, new StubEventCatalogClient(evt)).Handle(new PurchaseTicketsCommand(eventId, tierId, "a@b.com", 3), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Not enough tickets available.");
    }

    [Fact]
    public async Task PurchaseTicketsHandler_ShouldThrow_WhenEventOrTierIsMissing()
    {
        await using var db = TestDbContextFactory.Create();
        var eventId = Guid.NewGuid();
        var missingTierId = Guid.NewGuid();
        var evt = new EventCatalogItem(eventId, "Game", 2, Array.Empty<EventPriceTier>());

        await new Func<Task>(() => new PurchaseTicketsHandler(db, new StubEventCatalogClient(null)).Handle(new PurchaseTicketsCommand(eventId, missingTierId, "a@b.com", 1), CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>().WithMessage("Event not found.");
        await new Func<Task>(() => new PurchaseTicketsHandler(db, new StubEventCatalogClient(evt)).Handle(new PurchaseTicketsCommand(eventId, missingTierId, "a@b.com", 1), CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>().WithMessage("Pricing tier not found.");
    }

    [Fact]
    public async Task GetSalesSummaryHandler_ShouldAggregateOrders()
    {
        await using var db = TestDbContextFactory.Create();
        var eventId = Guid.NewGuid();
        db.Orders.Add(new TicketOrder(eventId, "one@example.com", 2, 10m));
        db.Orders.Add(new TicketOrder(eventId, "two@example.com", 3, 20m));
        db.Orders.Add(new TicketOrder(Guid.NewGuid(), "other@example.com", 9, 99m));
        await db.SaveChangesAsync();

        var dto = await new GetSalesSummaryHandler(db).Handle(new GetSalesSummaryQuery(eventId), CancellationToken.None);

        dto.Should().Be(new EventSalesSummaryDto(eventId, 2, 5, 80m));
    }
}
