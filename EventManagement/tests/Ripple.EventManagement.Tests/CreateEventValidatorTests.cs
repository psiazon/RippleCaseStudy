using FluentAssertions;
using Ripple.EventManagement.Application.Events;
using Xunit;

namespace Ripple.EventManagement.Tests;

public sealed class CreateEventValidatorTests
{
    [Fact]
    public void Should_fail_when_name_is_empty()
    {
        var validator = new CreateEventValidator();
        var command = new CreateEventCommand("", "desc", "venue", DateTimeOffset.MaxValue, TimeOnly.MinValue, 100, [new PricingTierRequest("General", 10)]);
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }
}
