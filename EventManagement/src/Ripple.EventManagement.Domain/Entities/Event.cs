namespace Ripple.EventManagement.Domain.Entities;

public sealed class Event
{
    private readonly List<PricingTier> _pricingTiers = new();
    private List<PricingTier> pricingTiers;

    private Event() { }

    public Event(string name, string description, string venue, DateTimeOffset eventDate, TimeOnly eventTime, int totalTicketCapacity, List<PricingTier> pricingTiers)
    {
        Id = Guid.NewGuid();
        Update(name, description, venue, eventDate, eventTime, totalTicketCapacity, pricingTiers);
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Venue { get; private set; } = string.Empty;
    public DateTimeOffset EventDate { get; private set; }

    public TimeOnly EventTime { get; private set; }
    public int TotalTicketCapacity { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public List<PricingTier> PricingTiers { get => pricingTiers; set => pricingTiers = value; }
    public void Update(string name, string description, string venue, DateTimeOffset eventDate, TimeOnly eventTime, int totalTicketCapacity, IEnumerable<PricingTier> pricingTiers)
    {
        if (totalTicketCapacity <= 0) throw new ArgumentOutOfRangeException(nameof(totalTicketCapacity));
        Name = name.Trim();
        Description = description.Trim();
        Venue = venue.Trim();
        EventDate = eventDate;
        EventTime = eventTime;
        TotalTicketCapacity = totalTicketCapacity;
        _pricingTiers.Clear();
        _pricingTiers.AddRange(pricingTiers);
    }
}
