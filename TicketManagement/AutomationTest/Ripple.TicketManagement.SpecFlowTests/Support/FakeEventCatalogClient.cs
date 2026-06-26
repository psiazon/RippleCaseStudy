using System.Collections.Concurrent;
using Ripple.TicketManagement.Application.Abstractions;

namespace Ripple.TicketManagement.SpecFlowTests.Support;

public sealed class FakeEventCatalogClient : IEventCatalogClient
{
    private readonly ConcurrentDictionary<Guid, EventCatalogItem> events = new();

    public void AddEvent(EventCatalogItem item) => events[item.Id] = item;

    public Task<EventCatalogItem?> GetEventAsync(Guid eventId, CancellationToken cancellationToken)
    {
        events.TryGetValue(eventId, out var item);
        return Task.FromResult(item);
    }
}
