using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ripple.EventManagement.Application.Events;
using System.Net.Http.Headers;

namespace Ripple.EventManagement.Api.Controllers
{
    [ApiController]
    [Route("api/events")]
    public sealed class EventsController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly IHttpClientFactory _httpClientFactory;

        public ISender Object { get; }

        public EventsController(ISender sender, IHttpClientFactory httpClientFactory)
        {
            _sender = sender;
            _httpClientFactory = httpClientFactory;
        }


        [HttpGet]
        [Authorize(Policy = "CanReadEvents")]
        public async Task<ActionResult<IReadOnlyCollection<EventDto>>> Get(CancellationToken cancellationToken) =>
            Ok(await _sender.Send(new GetEventsQuery(), cancellationToken));

        [HttpGet("{id:guid}")]
        [Authorize(Policy = "CanReadEvents")]
        public async Task<ActionResult<EventDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var evt = await _sender.Send(new GetEventByIdQuery(id), cancellationToken);
            return evt is null ? NotFound() : Ok(evt);
        }

        [HttpPost]
        [Authorize(Policy = "CanManageEvents")]
        public async Task<ActionResult<EventDto>> Create(CreateEventCommand command, CancellationToken cancellationToken)
        {
            var created = await _sender.Send(command, cancellationToken);

            try
            {
                var client = _httpClientFactory.CreateClient("TicketInventory"); // or CreateClient() if not named
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader))
                {
                    // authHeader expected like "Bearer <token>"
                    client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader);
                }

                var payload = new
                {
                    EventId = created.Id,
                    TotalCapacity = created.TotalTicketCapacity                    
                };
                var resp = await client.PostAsJsonAsync("api/tickets/inventories", payload, cancellationToken);
                resp.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                // log the failure; do not fail the create operation unless required
            }

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "CanManageEvents")]
        public async Task<IActionResult> Update(Guid id, UpdateEventCommand command, CancellationToken cancellationToken)
        {
            var updated = await _sender.Send(
                new UpdateEventCommand(
                    id,
                    command.Name,
                    command.Description,
                    command.Venue,
                    command.EventDate,
                    command.EventTime,
                    command.TotalTicketCapacity,
                    command.PricingTiers
                ),
                cancellationToken
            );
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "CanManageEvents")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var deleted = await _sender.Send(new DeleteEventCommand(id), cancellationToken);
            return deleted ? NoContent() : NotFound();
        }
    }
}
