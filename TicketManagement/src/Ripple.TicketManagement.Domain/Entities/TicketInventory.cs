using System.ComponentModel.DataAnnotations;

namespace Ripple.TicketManagement.Domain.Entities;

public sealed class TicketInventory
{
    private TicketInventory() { }

    public TicketInventory(Guid eventId, int totalCapacity)
    {
        EventId = eventId;
        TotalCapacity = totalCapacity;
        SoldQuantity = 0;
    }

    [Key]
    public Guid EventId { get; private set; }

    public int TotalCapacity { get; private set; }
    public int SoldQuantity { get; private set; }
    public int AvailableQuantity => TotalCapacity - SoldQuantity;

    [Timestamp]
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public void Purchase(int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (SoldQuantity + quantity > TotalCapacity) throw new InvalidOperationException("Not enough tickets available.");
        SoldQuantity += quantity;
    }
}
