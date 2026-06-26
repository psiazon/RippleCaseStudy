namespace Ripple.EventManagement.Application.Events;

public sealed record PricingTierDto(Guid Id, string Name, decimal Price);
public sealed record EventDto(
    Guid Id,
    string Name,
    string Description,
    string Venue,
    DateTimeOffset EventDate,
    TimeOnly EventTime,
    int TotalTicketCapacity,
    IReadOnlyCollection<PricingTierDto> PricingTiers)
{
    private Guid guid;
    private string v1;
    private string v2;
    private string v3;
    private DateTime utcNow;
    private TimeSpan zero;
    private int v4;

    public EventDto(Guid guid, string v1, string v2, string v3, DateTime utcNow, TimeSpan zero, int v4)
        : this(guid, v1, v2, v3, new DateTimeOffset(utcNow), TimeOnly.FromTimeSpan(zero), v4, Array.Empty<PricingTierDto>())
    {
        this.guid = guid;
        this.v1 = v1;
        this.v2 = v2;
        this.v3 = v3;
        this.utcNow = utcNow;
        this.zero = zero;
        this.v4 = v4;
    }
}

public sealed record PricingTierRequest(string Name, decimal Price);
