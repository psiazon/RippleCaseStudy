using FluentValidation;
using MediatR;
using Ripple.EventManagement.Application.Abstractions;
using Ripple.EventManagement.Domain.Entities;

namespace Ripple.EventManagement.Application.Events;

public sealed record CreateEventCommand(string Name, string Description, string Venue, DateTimeOffset EventDate, TimeOnly EventTime, int TotalTicketCapacity, IReadOnlyCollection<PricingTierRequest> PricingTiers) : IRequest<EventDto>;

public sealed class CreateEventValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Venue).NotEmpty().MaximumLength(300);
        RuleFor(x => x.EventDate).GreaterThan(DateTimeOffset.UtcNow.AddMinutes(-5));
        RuleFor(x => x.TotalTicketCapacity).GreaterThan(0).LessThanOrEqualTo(100000);
        RuleFor(x => x.PricingTiers).NotEmpty();
        RuleForEach(x => x.PricingTiers).ChildRules(tier =>
        {
            tier.RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            tier.RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        });
    }
}

public sealed class CreateEventHandler(IEventDbContext db) : IRequestHandler<CreateEventCommand, EventDto>
{
    public async Task<EventDto> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var tiers = request.PricingTiers.Select(x => new PricingTier(x.Name, x.Price)).ToList();
        var evt = new Event(request.Name, request.Description, request.Venue, request.EventDate, request.EventTime, request.TotalTicketCapacity, tiers);
        db.Events.Add(evt);
        await db.SaveChangesAsync(cancellationToken);
        return evt.ToDto();
    }
}
