namespace Ripple.TicketManagement.Application.Abstractions;

public interface IEventCatalogClient
{
    Task<EventCatalogItem?> GetEventAsync(Guid eventId, CancellationToken cancellationToken);
}

public sealed record EventCatalogItem(Guid Id, string Name, int TotalCapacity, IReadOnlyCollection<EventPriceTier> PricingTiers);
public sealed record EventPriceTier(Guid Id, string Name, decimal Price);
