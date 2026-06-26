namespace Ripple.TicketManagement.Application.Tickets;

public sealed record AvailabilityDto(Guid EventId, int TotalCapacity, int SoldQuantity, int AvailableQuantity);
public sealed record TicketOrderDto(Guid Id, Guid EventId, string PurchaserEmail, int Quantity, decimal UnitPrice, decimal TotalPrice, DateTimeOffset PurchasedAtUtc);
public sealed record EventSalesSummaryDto(Guid EventId, int OrdersCount, int TicketsSold, decimal GrossSales);
