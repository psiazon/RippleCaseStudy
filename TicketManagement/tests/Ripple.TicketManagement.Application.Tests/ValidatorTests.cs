using FluentAssertions;
using Ripple.TicketManagement.Application.Tickets;
using Xunit;

namespace Ripple.TicketManagement.Application.Tests;

public sealed class ValidatorTests
{
    [Fact]
    public void PurchaseTicketsValidator_ShouldAcceptValidCommand()
    {
        var command = new PurchaseTicketsCommand(Guid.NewGuid(), Guid.NewGuid(), "buyer@example.com", 2);

        var result = new PurchaseTicketsValidator().Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void PurchaseTicketsValidator_ShouldRejectInvalidCommand()
    {
        var command = new PurchaseTicketsCommand(Guid.Empty, Guid.Empty, "not-email", 21);

        var result = new PurchaseTicketsValidator().Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Select(x => x.PropertyName).Should().Contain(new[] { "EventId", "PricingTierId", "PurchaserEmail", "Quantity" });
    }

    [Fact]
    public void CreateInventoryValidator_ShouldValidateCapacityAndEventId()
    {
        var result = new CreateInventoryValidator().Validate(new CreateInventoryCommand(Guid.Empty, 0));

        result.IsValid.Should().BeFalse();
        result.Errors.Select(x => x.PropertyName).Should().Contain(new[] { "EventId", "TotalCapacity" });
    }
}
