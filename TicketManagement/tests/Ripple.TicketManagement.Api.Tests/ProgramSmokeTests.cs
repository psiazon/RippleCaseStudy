using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ripple.TicketManagement.Api.Tests;

public sealed class ProgramSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProgramSmokeTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task HealthEndpoint_ShouldReturnSuccess()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/health");

        response.IsSuccessStatusCode.Should().BeTrue();
    }
}
