using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Ripple.EventManagement.SpecFlowTests.Support;
using TechTalk.SpecFlow;

namespace Ripple.EventManagement.SpecFlowTests.StepDefinitions;

[Binding]
public sealed class EventManagementSteps
{
    private readonly SpecFlowTestContext _context;

    public EventManagementSteps(SpecFlowTestContext context)
    {
        _context = context;
    }

    [Given("I am authenticated as an \"(.*)\"")]
    public void GivenIAmAuthenticatedAs(string role)
    {
        _context.AuthenticateAs(role);
    }

    [Given("I am not authenticated")]
    public void GivenIAmNotAuthenticated()
    {
        _context.ClearAuthentication();
    }

    [Given("an event exists named \"(.*)\"")]
    public async Task GivenAnEventExistsNamed(string eventName)
    {
        _context.AuthenticateAs("EventManager");
        var request = BuildEventRequest(eventName);
        _context.LastResponse = await _context.Client.PostAsJsonAsync("/api/events", request);
        _context.LastResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        await _context.CaptureJsonAsync();
        _context.LastEventId = _context.LastJson.GetProperty("id").GetGuid();
    }

    [When("I create an event with the following values")]
    public async Task WhenICreateAnEventWithTheFollowingValues(Table table)
    {
        var row = table.Rows.Single();
        var request = BuildEventRequest(
            row["name"],
            row["description"],
            row["venue"],
            int.Parse(row["capacity"]),
            row["tierName"],
            decimal.Parse(row["tierPrice"]));

        _context.LastResponse = await _context.Client.PostAsJsonAsync("/api/events", request);
        await _context.CaptureJsonAsync();
        if (_context.LastResponse.StatusCode == HttpStatusCode.Created)
        {
            _context.LastEventId = _context.LastJson.GetProperty("id").GetGuid();
        }
    }

    [When("I create an invalid event with no name and zero capacity")]
    public async Task WhenICreateAnInvalidEventWithNoNameAndZeroCapacity()
    {
        var request = BuildEventRequest(name: string.Empty, capacity: 0, tierPrice: -1);
        _context.LastResponse = await _context.Client.PostAsJsonAsync("/api/events", request);
        await _context.CaptureJsonAsync();
    }

    [When("I request all events")]
    public async Task WhenIRequestAllEvents()
    {
        _context.LastResponse = await _context.Client.GetAsync("/api/events");
        await _context.CaptureJsonAsync();
    }

    [When("I update the event name to \"(.*)\"")]
    public async Task WhenIUpdateTheEventNameTo(string eventName)
    {
        var request = BuildEventRequest(eventName);
        _context.LastResponse = await _context.Client.PutAsJsonAsync($"/api/events/{_context.LastEventId}", request);
        await _context.CaptureJsonAsync();
    }

    [When("I delete the event")]
    public async Task WhenIDeleteTheEvent()
    {
        _context.LastResponse = await _context.Client.DeleteAsync($"/api/events/{_context.LastEventId}");
        await _context.CaptureJsonAsync();
    }

    [Then("the response status code should be (.*)")]
    public void ThenTheResponseStatusCodeShouldBe(int expectedStatusCode)
    {
        _context.LastResponse.Should().NotBeNull();
        ((int)_context.LastResponse!.StatusCode).Should().Be(expectedStatusCode);
    }

    [Then("the event response should contain \"(.*)\"")]
    public async Task ThenTheEventResponseShouldContain(string expectedText)
    {
        if (_context.LastResponse is null || _context.LastResponse.StatusCode == HttpStatusCode.NoContent)
        {
            _context.LastResponse = await _context.Client.GetAsync($"/api/events/{_context.LastEventId}");
        }

        var body = await _context.LastResponse!.Content.ReadAsStringAsync();
        body.Should().Contain(expectedText);
    }

    [Then("the created event should be available by id")]
    public async Task ThenTheCreatedEventShouldBeAvailableById()
    {
        var response = await _context.Client.GetAsync($"/api/events/{_context.LastEventId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Then("the event list should contain \"(.*)\"")]
    public async Task ThenTheEventListShouldContain(string expectedEventName)
    {
        var body = await _context.LastResponse!.Content.ReadAsStringAsync();
        body.Should().Contain(expectedEventName);
    }

    [Then("requesting the event by id should return (.*)")]
    public async Task ThenRequestingTheEventByIdShouldReturn(int expectedStatusCode)
    {
        var response = await _context.Client.GetAsync($"/api/events/{_context.LastEventId}");
        ((int)response.StatusCode).Should().Be(expectedStatusCode);
    }

    private static object BuildEventRequest(
        string name,
        string description = "Automated SpecFlow test event",
        string venue = "Wintrust Sports Complex",
        int capacity = 250,
        string tierName = "General Admission",
        decimal tierPrice = 20.00m)
    {
        return new
        {
            name,
            description,
            venue,
            eventDate = DateTimeOffset.UtcNow.AddDays(30),
            eventTime = "10:00:00",
            totalTicketCapacity = capacity,
            pricingTiers = new[]
            {
                new
                {
                    name = tierName,
                    price = tierPrice
                }
            }
        };
    }
}
