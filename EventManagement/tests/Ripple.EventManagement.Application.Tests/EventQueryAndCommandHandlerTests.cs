using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Ripple.EventManagement.Application.Events;
using Ripple.EventManagement.Domain.Entities;
using Xunit;

namespace Ripple.EventManagement.Application.Tests;

public sealed class EventQueryAndCommandHandlerTests
{
    [Fact]
    public async Task GetEvents_Should_return_events_ordered_by_event_date()
    {
        await using var db = TestDbContextFactory.Create();
        var late = new Event("Late", "", "Venue", DateTimeOffset.UtcNow.AddDays(10), TimeOnly.MinValue, 100, [new PricingTier("General", 10)]);
        var early = new Event("Early", "", "Venue", DateTimeOffset.UtcNow.AddDays(1), TimeOnly.MinValue, 100, [new PricingTier("General", 10)]);
        db.Events.AddRange(late, early);
        await db.SaveChangesAsync(CancellationToken.None);

        var result = await new GetEventsHandler(db).Handle(new GetEventsQuery(), CancellationToken.None);

        result.Select(e => e.Name).Should().Equal("Early", "Late");        
    }

    [Fact]
    public async Task GetEventById_Should_return_matching_event_or_null()
    {
        await using var db = TestDbContextFactory.Create();
        var evt = new Event("Target", "Description", "Venue", DateTimeOffset.UtcNow.AddDays(5), new TimeOnly(12, 0), 50, [new PricingTier("General", 15)]);
        db.Events.Add(evt);
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new GetEventByIdHandler(db);

        var found = await handler.Handle(new GetEventByIdQuery(evt.Id), CancellationToken.None);
        var missing = await handler.Handle(new GetEventByIdQuery(Guid.NewGuid()), CancellationToken.None);

        found.Should().NotBeNull();
        found!.Id.Should().Be(evt.Id);        
        missing.Should().BeNull();
    }

    [Fact]
    public async Task UpdateEvent_Should_return_false_when_event_is_missing()
    {
        await using var db = TestDbContextFactory.Create();
        var command = new UpdateEventCommand(Guid.NewGuid(), "Name", "Description", "Venue", DateTimeOffset.UtcNow.AddDays(2), TimeOnly.MinValue, 100, [new PricingTierRequest("General", 10)]);

        var result = await new UpdateEventHandler(db).Handle(command, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteEvent_Should_remove_existing_event_and_return_true()
    {
        await using var db = TestDbContextFactory.Create();
        var evt = new Event("Delete", "Description", "Venue", DateTimeOffset.UtcNow.AddDays(1), TimeOnly.MinValue, 10, [new PricingTier("General", 1)]);
        db.Events.Add(evt);
        await db.SaveChangesAsync(CancellationToken.None);

        var result = await new DeleteEventHandler(db).Handle(new DeleteEventCommand(evt.Id), CancellationToken.None);

        result.Should().BeTrue();
        db.Events.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteEvent_Should_return_false_when_event_is_missing()
    {
        await using var db = TestDbContextFactory.Create();

        var result = await new DeleteEventHandler(db).Handle(new DeleteEventCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeFalse();
    }
}
