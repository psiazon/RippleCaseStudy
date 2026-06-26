using MediatR;
using Microsoft.EntityFrameworkCore;
using Ripple.EventManagement.Application.Abstractions;

namespace Ripple.EventManagement.Application.Events;

public sealed record GetEventsQuery : IRequest<IReadOnlyCollection<EventDto>>;
public sealed record GetEventByIdQuery(Guid Id) : IRequest<EventDto?>;

public sealed class GetEventsHandler(IEventDbContext db) : IRequestHandler<GetEventsQuery, IReadOnlyCollection<EventDto>>
{
    public async Task<IReadOnlyCollection<EventDto>> Handle(GetEventsQuery request, CancellationToken cancellationToken) =>
        await db.Events.AsNoTracking().Include(x => x.PricingTiers).OrderBy(x => x.EventDate).Select(x => x.ToDto()).ToListAsync(cancellationToken);
}

public sealed class GetEventByIdHandler(IEventDbContext db) : IRequestHandler<GetEventByIdQuery, EventDto?>
{
    public async Task<EventDto?> Handle(GetEventByIdQuery request, CancellationToken cancellationToken) =>
        await db.Events.AsNoTracking().Include(x => x.PricingTiers).Where(x => x.Id == request.Id).Select(x => x.ToDto()).SingleOrDefaultAsync(cancellationToken);
}
