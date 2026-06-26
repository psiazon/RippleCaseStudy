namespace Ripple.TicketManagement.Domain.Entities;

public sealed class TicketOrder
{
    private TicketOrder() { }

    public TicketOrder(Guid eventId, string purchaserEmail, int quantity, decimal unitPrice)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        PurchaserEmail = purchaserEmail.Trim().ToLowerInvariant();
        Quantity = quantity;
        UnitPrice = unitPrice;
        PurchasedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public string PurchaserEmail { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice => Quantity * UnitPrice;
    public DateTimeOffset PurchasedAtUtc { get; private set; }
}
