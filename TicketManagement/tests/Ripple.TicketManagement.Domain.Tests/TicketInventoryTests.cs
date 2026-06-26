using FluentAssertions;
using Ripple.TicketManagement.Domain.Entities;
using Xunit;

namespace Ripple.TicketManagement.Domain.Tests;

public sealed class TicketInventoryTests
{
    [Fact]
    public void Constructor_ShouldInitializeInventory()
    {
        var eventId = Guid.NewGuid();

        var inventory = new TicketInventory(eventId, 100);

        inventory.EventId.Should().Be(eventId);
        inventory.TotalCapacity.Should().Be(100);
        inventory.SoldQuantity.Should().Be(0);
        inventory.AvailableQuantity.Should().Be(100);
        inventory.RowVersion.Should().BeEmpty();
    }

    [Fact]
    public void Purchase_ShouldReduceAvailableQuantity()
    {
        var inventory = new TicketInventory(Guid.NewGuid(), 10);

        inventory.Purchase(3);

        inventory.SoldQuantity.Should().Be(3);
        inventory.AvailableQuantity.Should().Be(7);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Purchase_ShouldRejectNonPositiveQuantity(int quantity)
    {
        var inventory = new TicketInventory(Guid.NewGuid(), 10);

        var act = () => inventory.Purchase(quantity);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("quantity");
    }

    [Fact]
    public void Purchase_ShouldPreventOverselling()
    {
        var inventory = new TicketInventory(Guid.NewGuid(), 2);

        var act = () => inventory.Purchase(3);

        act.Should().Throw<InvalidOperationException>().WithMessage("Not enough tickets available.");
        inventory.SoldQuantity.Should().Be(0);
    }
}
