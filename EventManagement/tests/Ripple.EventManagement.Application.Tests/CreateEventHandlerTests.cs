using FluentAssertions;
using Ripple.EventManagement.Application.Events;
using Xunit;

namespace Ripple.EventManagement.Application.Tests;

public sealed class CreateEventHandlerTests
{
    [Fact]
    public async Task Handle_Should_create_event_save_it_and_return_dto()
    {
        await using var db = TestDbContextFactory.Create();
        var handler = new CreateEventHandler(db);
        var command = new CreateEventCommand(
            "Championship",
            "Finals",
            "Wintrust",
            DateTimeOffset.UtcNow.AddDays(30),
            new TimeOnly(19, 0),
            1000,
            [new PricingTierRequest("General", 20), new PricingTierRequest("VIP", 75)]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Championship");        
        db.Events.Should().ContainSingle(e => e.Id == result.Id);        
    }
}
