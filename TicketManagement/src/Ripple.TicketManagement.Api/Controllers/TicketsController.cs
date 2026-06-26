using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ripple.TicketManagement.Application.Tickets;

namespace Ripple.TicketManagement.Api.Controllers;

[ApiController]
[Route("api/tickets")]
public sealed class TicketsController(ISender sender) : ControllerBase
{
    [HttpPost("purchase")]
    [Authorize(Policy = "CanBuyTickets")]
    public async Task<ActionResult<TicketOrderDto>> Purchase(PurchaseTicketsCommand command, CancellationToken cancellationToken) =>
        Ok(await sender.Send(command, cancellationToken));

    [HttpPost("inventories")]
    [Authorize(Policy = "CanViewReports")]
    public async Task<IActionResult> CreateInventory(CreateInventoryCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Availability), new { eventId = command.EventId }, null);
    }

    [HttpGet("availability/{eventId:guid}")]
    [Authorize(Policy = "CanBuyTickets")]
    public async Task<ActionResult<AvailabilityDto>> Availability(Guid eventId, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetAvailabilityQuery(eventId), cancellationToken));

    [HttpGet("reports/sales/{eventId:guid}")]
    [Authorize(Policy = "CanViewReports")]
    public async Task<ActionResult<EventSalesSummaryDto>> Sales(Guid eventId, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetSalesSummaryQuery(eventId), cancellationToken));
}
