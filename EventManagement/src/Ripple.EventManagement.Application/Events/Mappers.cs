using Ripple.EventManagement.Domain.Entities;

namespace Ripple.EventManagement.Application.Events;

public static class Mappers
{
    public static EventDto ToDto(this Event evt) => new(evt.Id, evt.Name, evt.Description, evt.Venue, evt.EventDate, evt.EventTime, evt.TotalTicketCapacity, evt.PricingTiers.Select(x => new PricingTierDto(x.Id, x.Name, x.Price)).ToList());
}
