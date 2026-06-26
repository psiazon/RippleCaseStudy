using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ripple.EventManagement.Application.Abstractions;
using Ripple.EventManagement.Domain.Entities;

namespace Ripple.EventManagement.Application.Events;

using System.Text.Json.Serialization;

public sealed class UpdateEventCommand : IRequest<bool>
{
    [JsonConstructor]
    public UpdateEventCommand(
        Guid id,
        string name,
        string description,
        string venue,
        DateTimeOffset eventDate,
        TimeOnly eventTime,
        int totalTicketCapacity,
        PricingTierRequest[] pricingTiers)
    {
        Id = id;
        Name = name;
        Description = description;
        Venue = venue;
        EventDate = eventDate;
        EventTime = eventTime;
        TotalTicketCapacity = totalTicketCapacity;
        PricingTiers = pricingTiers;
    }

    public Guid Id { get; }
    public string Name { get; }
    public string Description { get; }
    public string Venue { get; }
    public DateTimeOffset EventDate { get; }
    public TimeOnly EventTime { get; }
    public int TotalTicketCapacity { get; }
    public PricingTierRequest[] PricingTiers { get; }
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
