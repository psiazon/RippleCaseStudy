using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Ripple.TicketManagement.Application.Abstractions;
using Ripple.TicketManagement.Application.Tickets;
using Ripple.TicketManagement.SpecFlowTests.Support;
using TechTalk.SpecFlow;

namespace Ripple.TicketManagement.SpecFlowTests.StepDefinitions;

[Binding]
public sealed class TicketApiSteps : IDisposable
{
    private readonly ScenarioContext scenarioContext;
    private readonly TicketManagementApiFactory factory;
    private readonly HttpClient client;

    public TicketApiSteps(ScenarioContext scenarioContext)
    {
        this.scenarioContext = scenarioContext;
        factory = new TicketManagementApiFactory();
        client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Given("the event catalog contains an event named \"(.*)\" with (.*) total tickets and a \"(.*)\" price tier costing (.*)")]
    public void GivenTheEventCatalogContainsAnEvent(string eventName, int totalTickets, string tierName, decimal price)
    {
        var eventId = Guid.NewGuid();
        var pricingTierId = Guid.NewGuid();
        factory.EventCatalog.AddEvent(new EventCatalogItem(
            eventId,
            eventName,
            totalTickets,
            new[] { new EventPriceTier(pricingTierId, tierName, price) }));

        scenarioContext[ScenarioContextKeys.EventId] = eventId;
        scenarioContext[ScenarioContextKeys.PricingTierId] = pricingTierId;
    }

    [Given("a \"(.*)\" already purchased (.*) tickets for \"(.*)\"")]
    public async Task GivenUserAlreadyPurchasedTickets(string role, int quantity, string purchaserEmail)
    {
        await PurchaseTicketsAsync(role, quantity, purchaserEmail);
        LastResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [When("a \"(.*)\" purchases (.*) tickets for \"(.*)\"")]
    public async Task WhenUserPurchasesTickets(string role, int quantity, string purchaserEmail) =>
        await PurchaseTicketsAsync(role, quantity, purchaserEmail);

    [When("an unauthenticated user purchases (.*) tickets for \"(.*)\"")]
    public async Task WhenUnauthenticatedUserPurchasesTickets(int quantity, string purchaserEmail)
    {
        client.DefaultRequestHeaders.Authorization = null;
        await SendPurchaseRequestAsync(quantity, purchaserEmail);
    }

    [When("a \"(.*)\" checks ticket availability")]
    public async Task WhenUserChecksTicketAvailability(string role)
    {
        AuthorizeAs(role);
        var eventId = GetEventId();
        await StoreResponseAsync(await client.GetAsync($"/api/tickets/availability/{eventId}"));
    }

    [When("a \"(.*)\" requests the sales summary")]
    public async Task WhenUserRequestsTheSalesSummary(string role)
    {
        AuthorizeAs(role);
        var eventId = GetEventId();
        await StoreResponseAsync(await client.GetAsync($"/api/tickets/reports/sales/{eventId}"));
    }

    [When("a \"(.*)\" creates inventory with (.*) total tickets")]
    public async Task WhenUserCreatesInventory(string role, int totalTickets)
    {
        AuthorizeAs(role);
        var command = new CreateInventoryCommand(GetEventId(), totalTickets);
        await StoreResponseAsync(await client.PostAsJsonAsync("/api/tickets/inventories", command));
    }

    [Then("the response status code should be (.*)")]
    public void ThenTheResponseStatusCodeShouldBe(int statusCode) =>
        LastResponse.StatusCode.Should().Be((HttpStatusCode)statusCode, LastResponseBody);

    [Then("the ticket order should contain (.*) tickets at (.*) each")]
    public async Task ThenTheTicketOrderShouldContainTicketsAtEach(int quantity, decimal unitPrice)
    {
        var order = await LastResponse.Content.ReadFromJsonAsync<TicketOrderDto>();
        order.Should().NotBeNull();
        order!.Quantity.Should().Be(quantity);
        order.UnitPrice.Should().Be(unitPrice);
        order.TotalPrice.Should().Be(quantity * unitPrice);
    }

    [Then("the availability should show (.*) tickets remaining")]
    public async Task ThenTheAvailabilityShouldShowTicketsRemaining(int remainingTickets)
    {
        var availability = await LastResponse.Content.ReadFromJsonAsync<AvailabilityDto>();
        availability.Should().NotBeNull();
        availability!.AvailableQuantity.Should().Be(remainingTickets);
    }

    [Then("the sales summary should show (.*) orders, (.*) tickets sold, and (.*) gross sales")]
    public async Task ThenTheSalesSummaryShouldShow(int ordersCount, int ticketsSold, decimal grossSales)
    {
        var summary = await LastResponse.Content.ReadFromJsonAsync<EventSalesSummaryDto>();
        summary.Should().NotBeNull();
        summary!.OrdersCount.Should().Be(ordersCount);
        summary.TicketsSold.Should().Be(ticketsSold);
        summary.GrossSales.Should().Be(grossSales);
    }

    [Then("the error response should contain \"(.*)\"")]
    public void ThenTheErrorResponseShouldContain(string expectedText) =>
        LastResponseBody.Should().Contain(expectedText);

    public void Dispose()
    {
        client.Dispose();
        factory.Dispose();
    }

    private async Task PurchaseTicketsAsync(string role, int quantity, string purchaserEmail)
    {
        AuthorizeAs(role);
        await SendPurchaseRequestAsync(quantity, purchaserEmail);
    }

    private async Task SendPurchaseRequestAsync(int quantity, string purchaserEmail)
    {
        var command = new PurchaseTicketsCommand(GetEventId(), GetPricingTierId(), purchaserEmail, quantity);
        await StoreResponseAsync(await client.PostAsJsonAsync("/api/tickets/purchase", command));
    }

    private void AuthorizeAs(string role) =>
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JwtTokenFactory.CreateToken(role));

    private Guid GetEventId() => (Guid)scenarioContext[ScenarioContextKeys.EventId];

    private Guid GetPricingTierId() => (Guid)scenarioContext[ScenarioContextKeys.PricingTierId];

    private HttpResponseMessage LastResponse => (HttpResponseMessage)scenarioContext[ScenarioContextKeys.LastResponse];

    private string LastResponseBody => (string)scenarioContext[ScenarioContextKeys.LastResponseBody];

    private async Task StoreResponseAsync(HttpResponseMessage response)
    {
        scenarioContext[ScenarioContextKeys.LastResponse] = response;
        scenarioContext[ScenarioContextKeys.LastResponseBody] = await response.Content.ReadAsStringAsync();
    }
}
