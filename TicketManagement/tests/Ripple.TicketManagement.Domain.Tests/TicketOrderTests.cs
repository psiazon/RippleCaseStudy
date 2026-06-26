using FluentAssertions;
using Ripple.TicketManagement.Domain.Entities;
using Xunit;

namespace Ripple.TicketManagement.Domain.Tests;

public sealed class TicketOrderTests
{
    [Fact]
    public void Constructor_ShouldNormalizeEmailAndCalculateTotal()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        var eventId = Guid.NewGuid();

        var order = new TicketOrder(eventId, "  Buyer@Example.COM  ", 4, 12.50m);

        order.Id.Should().NotBeEmpty();
        order.EventId.Should().Be(eventId);
        order.PurchaserEmail.Should().Be("buyer@example.com");
        order.Quantity.Should().Be(4);
        order.UnitPrice.Should().Be(12.50m);
        order.TotalPrice.Should().Be(50.00m);
        order.PurchasedAtUtc.Should().BeOnOrAfter(before);
    }
}
