namespace Ripple.EventManagement.Domain.Entities;

public sealed class PricingTier
{
    private PricingTier() { }

    public PricingTier(string name, decimal price)
    {
        Id = Guid.NewGuid();
        Name = name.Trim();
        Price = price;
    }

    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
}
