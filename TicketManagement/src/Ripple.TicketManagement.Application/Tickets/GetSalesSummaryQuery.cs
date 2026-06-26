using MediatR;
using Microsoft.EntityFrameworkCore;
using Ripple.TicketManagement.Application.Abstractions;

namespace Ripple.TicketManagement.Application.Tickets;

public sealed record GetSalesSummaryQuery(Guid EventId) : IRequest<EventSalesSummaryDto>;

public sealed class GetSalesSummaryHandler(ITicketDbContext db) : IRequestHandler<GetSalesSummaryQuery, EventSalesSummaryDto>
{
    public async Task<EventSalesSummaryDto> Handle(GetSalesSummaryQuery request, CancellationToken cancellationToken)
    {
        var orders = await db.Orders.AsNoTracking().Where(x => x.EventId == request.EventId).ToListAsync(cancellationToken);
        return new EventSalesSummaryDto(request.EventId, orders.Count, orders.Sum(x => x.Quantity), orders.Sum(x => x.TotalPrice));
    }
}
