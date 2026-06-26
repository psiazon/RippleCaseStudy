using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Ripple.TicketManagement.Application.Abstractions;

namespace Ripple.TicketManagement.Infrastructure.Clients;

public sealed class EventCatalogClient(HttpClient httpClient, IHttpContextAccessor accessor) : IEventCatalogClient
{
    public async Task<EventCatalogItem?> GetEventAsync(Guid eventId, CancellationToken cancellationToken)
    {
        var bearer = accessor.HttpContext?.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrWhiteSpace(bearer)) httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(bearer);
        return await httpClient.GetFromJsonAsync<EventCatalogItem>($"api/events/{eventId}", cancellationToken);
    }
}
