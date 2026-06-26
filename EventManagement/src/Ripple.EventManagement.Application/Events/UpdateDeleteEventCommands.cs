using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ripple.EventManagement.Application.Abstractions;
using Ripple.EventManagement.Domain.Entities;

namespace Ripple.EventManagement.Application.Events;

public sealed record UpdateEventCommand(
    Guid Id,
    string Name,
    string Description,
    string Venue,
    DateTimeOffset EventDate,
    TimeOnly EventTime,
    int TotalTicketCapacity,
    IReadOnlyCollection<PricingTierRequest> PricingTiers
) : IRequest<bool>
{
    public UpdateEventCommand(
        Guid id,
        string v1,
        string v2,
        string v3,
        DateTime utcNow,
        TimeOnly timeOnly,
        int v4,
        PricingTierDto[] pricingTierDtos
    )
        : this(id, v1, v2, v3, utcNow, timeOnly, v4, pricingTierDtos.Select(dto => new PricingTierRequest(dto.Name, dto.Price)).ToArray())
    {
        // Additional initialization if needed
    }
}

public sealed record DeleteEventCommand(Guid Id) : IRequest<bool>;

public sealed class UpdateEventValidator : AbstractValidator<UpdateEventCommand>
{
    public UpdateEventValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Venue).NotEmpty().MaximumLength(300);
        RuleFor(x => x.TotalTicketCapacity).GreaterThan(0);
        RuleFor(x => x.PricingTiers).NotEmpty();
    }
}

public sealed class UpdateEventHandler(IEventDbContext db) : IRequestHandler<UpdateEventCommand, bool>
{
    public async Task<bool> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var evt = await db.Events.Include(x => x.PricingTiers).SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (evt is null) return false;
        evt.Update(request.Name, request.Description, request.Venue, request.EventDate, request.EventTime, request.TotalTicketCapacity, request.PricingTiers.Select(x => new PricingTier(x.Name, x.Price)));
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public sealed class DeleteEventHandler(IEventDbContext db) : IRequestHandler<DeleteEventCommand, bool>
{
    public async Task<bool> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        var evt = await db.Events.SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (evt is null) return false;
        db.Events.Remove(evt);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
