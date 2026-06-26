using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ripple.TicketManagement.Application.Abstractions;
using Ripple.TicketManagement.Domain.Entities;

namespace Ripple.TicketManagement.Application.Tickets;

public sealed record PurchaseTicketsCommand(Guid EventId, Guid PricingTierId, string PurchaserEmail, int Quantity) : IRequest<TicketOrderDto>;

public sealed class PurchaseTicketsValidator : AbstractValidator<PurchaseTicketsCommand>
{
    public PurchaseTicketsValidator()
    {
        RuleFor(x => x.EventId).NotEmpty();
        RuleFor(x => x.PricingTierId).NotEmpty();
        RuleFor(x => x.PurchaserEmail).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(20);
    }
}

public sealed class PurchaseTicketsHandler(ITicketDbContext db, IEventCatalogClient eventClient) : IRequestHandler<PurchaseTicketsCommand, TicketOrderDto>
{
    public async Task<TicketOrderDto> Handle(PurchaseTicketsCommand request, CancellationToken cancellationToken)
    {
        var evt = await eventClient.GetEventAsync(request.EventId, cancellationToken) ?? throw new InvalidOperationException("Event not found.");
        var tier = evt.PricingTiers.SingleOrDefault(x => x.Id == request.PricingTierId) ?? throw new InvalidOperationException("Pricing tier not found.");
        var inventory = await db.Inventories.SingleOrDefaultAsync(x => x.EventId == request.EventId, cancellationToken);
        if (inventory is null)
        {
            inventory = new TicketInventory(request.EventId, evt.TotalCapacity);
            db.Inventories.Add(inventory);
        }
        inventory.Purchase(request.Quantity);
        var order = new TicketOrder(request.EventId, request.PurchaserEmail, request.Quantity, tier.Price);
        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);
        return new TicketOrderDto(order.Id, order.EventId, order.PurchaserEmail, order.Quantity, order.UnitPrice, order.TotalPrice, order.PurchasedAtUtc);
    }
}
