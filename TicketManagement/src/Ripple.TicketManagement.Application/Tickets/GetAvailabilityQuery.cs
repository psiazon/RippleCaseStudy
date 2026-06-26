using MediatR;
using Microsoft.EntityFrameworkCore;
using Ripple.TicketManagement.Application.Abstractions;
using Ripple.TicketManagement.Domain.Entities;

namespace Ripple.TicketManagement.Application.Tickets;

public sealed record GetAvailabilityQuery(Guid EventId) : IRequest<AvailabilityDto>;

public sealed class GetAvailabilityHandler(ITicketDbContext db, IEventCatalogClient eventClient) : IRequestHandler<GetAvailabilityQuery, AvailabilityDto>
{
    public async Task<AvailabilityDto> Handle(GetAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var evt = await eventClient.GetEventAsync(request.EventId, cancellationToken) ?? throw new InvalidOperationException("Event not found.");
        var inventory = await db.Inventories.AsNoTracking().SingleOrDefaultAsync(x => x.EventId == request.EventId, cancellationToken) ?? new TicketInventory(request.EventId, evt.TotalCapacity);
        return new AvailabilityDto(request.EventId, inventory.TotalCapacity, inventory.SoldQuantity, inventory.AvailableQuantity);
    }
}
