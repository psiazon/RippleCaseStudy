using FluentAssertions;
using Ripple.EventManagement.Domain.Entities;
using Xunit;

namespace Ripple.EventManagement.Domain.Tests;

public sealed class EventEntityTests
{
    [Fact]
    public void Constructor_Should_trim_values_initialize_identity_and_add_pricing_tiers()
    {
        var tier = new PricingTier(" General ", 25.50m);
        var eventDate = DateTimeOffset.UtcNow.AddDays(10);
        var eventTime = new TimeOnly(18, 30);

        var evt = new Event("  Championship  ", "  Finals night  ", "  Wintrust  ", eventDate, eventTime, 250, [tier]);

        evt.Id.Should().NotBeEmpty();
        evt.Name.Should().Be("Championship");
        evt.Description.Should().Be("Finals night");
        evt.Venue.Should().Be("Wintrust");
        evt.EventDate.Should().Be(eventDate);
        evt.EventTime.Should().Be(eventTime);
        evt.TotalTicketCapacity.Should().Be(250);
        evt.CreatedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));        
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_Should_throw_when_capacity_is_not_positive(int capacity)
    {
        Action act = () => new Event("Name", "Description", "Venue", DateTimeOffset.UtcNow.AddDays(1), TimeOnly.MinValue, capacity, [new PricingTier("General", 10)]);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("totalTicketCapacity");
    }

    [Fact]
    public void Update_Should_replace_existing_values_and_pricing_tiers()
    {
        var evt = new Event("Old", "Old description", "Old venue", DateTimeOffset.UtcNow.AddDays(1), new TimeOnly(9, 0), 100, [new PricingTier("Old", 1)]);
        var newDate = DateTimeOffset.UtcNow.AddDays(20);
        var newTime = new TimeOnly(20, 15);
        var newTiers = new[] { new PricingTier("VIP", 100), new PricingTier("General", 30) };

        evt.Update(" New ", " New description ", " New venue ", newDate, newTime, 500, newTiers);

        evt.Name.Should().Be("New");
        evt.Description.Should().Be("New description");
        evt.Venue.Should().Be("New venue");
        evt.EventDate.Should().Be(newDate);
        evt.EventTime.Should().Be(newTime);
        evt.TotalTicketCapacity.Should().Be(500);
        
    }
}
