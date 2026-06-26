using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ripple.TicketManagement.Application.Abstractions;
using Ripple.TicketManagement.Infrastructure;
using Ripple.TicketManagement.Infrastructure.Clients;
using Ripple.TicketManagement.Infrastructure.Persistence;
using Xunit;

namespace Ripple.TicketManagement.Infrastructure.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddTicketInfrastructure_ShouldRegisterExpectedServices()
    {
        var values = new Dictionary<string, string?>
        {
            ["ConnectionStrings:TicketDb"] = "Server=(localdb)\\MSSQLLocalDB;Database=TicketTests;Trusted_Connection=True;",
            ["EventManagement:BaseUrl"] = "https://events.example/"
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var services = new ServiceCollection();

        services.AddTicketInfrastructure(configuration);

        services.Should().Contain(x => x.ServiceType == typeof(TicketDbContext));
        services.Should().Contain(x => x.ServiceType == typeof(ITicketDbContext));
        services.Should().Contain(x => x.ServiceType == typeof(IEventCatalogClient));        
    }
}
