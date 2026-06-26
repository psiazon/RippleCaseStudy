using FluentAssertions;
using Ripple.EventManagement.Domain.Entities;
using Xunit;

namespace Ripple.EventManagement.Domain.Tests;

public sealed class PricingTierTests
{
    [Fact]
    public void Constructor_Should_trim_name_set_price_and_generate_id()
    {
        var tier = new PricingTier("  Courtside  ", 125.75m);

        tier.Id.Should().NotBeEmpty();
        tier.Name.Should().Be("Courtside");
        tier.Price.Should().Be(125.75m);
        tier.EventId.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_Should_allow_zero_price_for_free_tier()
    {
        var tier = new PricingTier("Free", 0m);

        tier.Price.Should().Be(0m);
    }
}
