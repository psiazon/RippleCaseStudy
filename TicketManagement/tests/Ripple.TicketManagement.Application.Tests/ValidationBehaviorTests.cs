using FluentAssertions;
using FluentValidation;
using MediatR;
using Ripple.TicketManagement.Application.Common;
using Ripple.TicketManagement.Application.Tickets;
using Xunit;

namespace Ripple.TicketManagement.Application.Tests;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldCallNext_WhenNoValidatorsExist()
    {
        var behavior = new ValidationBehavior<GetAvailabilityQuery, AvailabilityDto>(Array.Empty<IValidator<GetAvailabilityQuery>>());
        var expected = new AvailabilityDto(Guid.NewGuid(), 10, 1, 9);

        var result = await behavior.Handle(new GetAvailabilityQuery(expected.EventId), _ => Task.FromResult(expected), CancellationToken.None);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenValidationFails()
    {
        var behavior = new ValidationBehavior<PurchaseTicketsCommand, TicketOrderDto>(new[] { new PurchaseTicketsValidator() });
        var command = new PurchaseTicketsCommand(Guid.Empty, Guid.Empty, "bad", 0);

        var act = () => behavior.Handle(command, _ => throw new InvalidOperationException("next should not run"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
