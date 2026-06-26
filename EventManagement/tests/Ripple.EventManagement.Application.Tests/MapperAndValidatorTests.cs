using FluentAssertions;
using FluentValidation;
using Ripple.EventManagement.Application.Common;
using Ripple.EventManagement.Application.Events;
using Ripple.EventManagement.Domain.Entities;
using Xunit;

namespace Ripple.EventManagement.Application.Tests;

public sealed class MapperAndValidatorTests
{
    [Fact]
    public void ToDto_Should_map_all_event_and_pricing_tier_fields()
    {
        var eventDate = DateTimeOffset.UtcNow.AddDays(7);
        var eventTime = new TimeOnly(15, 45);
        var evt = new Event("Name", "Description", "Venue", eventDate, eventTime, 123, [new PricingTier("General", 12.34m)]);

        var dto = evt.ToDto();

        dto.Id.Should().Be(evt.Id);
        dto.Name.Should().Be("Name");
        dto.Description.Should().Be("Description");
        dto.Venue.Should().Be("Venue");
        dto.EventDate.Should().Be(eventDate);
        dto.EventTime.Should().Be(eventTime);
        dto.TotalTicketCapacity.Should().Be(123);        
    }

    [Fact]
    public void CreateEventValidator_Should_accept_valid_command()
    {
        var command = new CreateEventCommand("Name", "Description", "Venue", DateTimeOffset.UtcNow.AddDays(1), TimeOnly.MinValue, 100, [new PricingTierRequest("General", 10)]);

        var result = new CreateEventValidator().Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateEventValidator_Should_report_expected_failures()
    {
        var command = new CreateEventCommand("", new string('x', 2001), "", DateTimeOffset.UtcNow.AddDays(-1), TimeOnly.MinValue, 0, [new PricingTierRequest("", -1)]);

        var result = new CreateEventValidator().Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Select(e => e.PropertyName).Should().Contain(["Name", "Description", "Venue", "EventDate", "TotalTicketCapacity", "PricingTiers[0].Name", "PricingTiers[0].Price"]);
    }

    [Fact]
    public void UpdateEventValidator_Should_accept_valid_command_and_reject_invalid_command()
    {
        var valid = new UpdateEventCommand(Guid.NewGuid(), "Name", "Description", "Venue", DateTimeOffset.UtcNow.AddDays(1), TimeOnly.MinValue, 100, [new PricingTierRequest("General", 10)]);
        var invalid = new UpdateEventCommand(Guid.Empty, "", "Description", "", DateTimeOffset.UtcNow.AddDays(1), TimeOnly.MinValue, 0, []);
        var validator = new UpdateEventValidator();

        validator.Validate(valid).IsValid.Should().BeTrue();
        validator.Validate(invalid).IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidationBehavior_Should_call_next_when_no_validators_are_registered()
    {
        var behavior = new ValidationBehavior<CreateEventCommand, string>([]);
        var called = false;
        var request = new CreateEventCommand("Name", "Description", "Venue", DateTimeOffset.UtcNow.AddDays(1), TimeOnly.MinValue, 100, [new PricingTierRequest("General", 10)]);

        var result = await behavior.Handle(request, _ => { called = true; return Task.FromResult("ok"); }, CancellationToken.None);

        result.Should().Be("ok");
        called.Should().BeTrue();
    }

    [Fact]
    public async Task ValidationBehavior_Should_throw_when_validator_fails()
    {
        var behavior = new ValidationBehavior<CreateEventCommand, string>([new CreateEventValidator()]);
        var request = new CreateEventCommand("", "Description", "Venue", DateTimeOffset.UtcNow.AddDays(1), TimeOnly.MinValue, 100, [new PricingTierRequest("General", 10)]);

        Func<Task> act = () => behavior.Handle(request, _ => Task.FromResult("should-not-run"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
