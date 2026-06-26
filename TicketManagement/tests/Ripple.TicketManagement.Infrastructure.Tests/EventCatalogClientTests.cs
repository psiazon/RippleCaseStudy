using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Ripple.TicketManagement.Application.Abstractions;
using Ripple.TicketManagement.Infrastructure.Clients;
using Xunit;

namespace Ripple.TicketManagement.Infrastructure.Tests;

public sealed class EventCatalogClientTests
{
    [Fact]
    public async Task GetEventAsync_ShouldForwardAuthorizationHeaderAndDeserializeEvent()
    {
        var eventId = Guid.NewGuid();
        var expected = new EventCatalogItem(eventId, "Championship", 100, new[] { new EventPriceTier(Guid.NewGuid(), "GA", 12m) });
        using var handler = new CapturingHandler(JsonSerializer.Serialize(expected));
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://events.example/") };
        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = "Bearer test-token";
        var accessor = new HttpContextAccessor { HttpContext = context };
        var client = new EventCatalogClient(httpClient, accessor);

        var result = await client.GetEventAsync(eventId, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        handler.RequestUri!.ToString().Should().Be($"https://events.example/api/events/{eventId}");
        handler.AuthorizationHeader.Should().Be("Bearer test-token");
    }

    [Fact]
    public async Task GetEventAsync_ShouldNotRequireAuthorizationHeader()
    {
        var eventId = Guid.NewGuid();
        using var handler = new CapturingHandler("null");
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://events.example/") };
        var client = new EventCatalogClient(httpClient, new HttpContextAccessor());

        var result = await client.GetEventAsync(eventId, CancellationToken.None);

        result.Should().BeNull();
        handler.AuthorizationHeader.Should().BeNull();
    }

    private sealed class CapturingHandler(string responseJson) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }
        public string? AuthorizationHeader { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            AuthorizationHeader = request.Headers.Authorization?.ToString();
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, System.Text.Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}
