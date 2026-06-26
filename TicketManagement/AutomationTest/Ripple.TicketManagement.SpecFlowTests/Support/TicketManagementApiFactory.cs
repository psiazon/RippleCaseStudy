using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ripple.TicketManagement.Application.Abstractions;
using Ripple.TicketManagement.Infrastructure.Persistence;

namespace Ripple.TicketManagement.SpecFlowTests.Support;

public sealed class TicketManagementApiFactory : WebApplicationFactory<Program>
{
    public FakeEventCatalogClient EventCatalog { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = JwtTokenFactory.Issuer,
                ["Jwt:Audience"] = JwtTokenFactory.Audience,
                ["Jwt:SigningKey"] = JwtTokenFactory.SigningKey,
                ["Cors:AllowedOrigins:0"] = "https://localhost"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<TicketDbContext>>();
            services.RemoveAll<TicketDbContext>();
            services.RemoveAll<ITicketDbContext>();
            services.RemoveAll<IEventCatalogClient>();

            services.AddDbContext<TicketDbContext>(options =>
                options.UseInMemoryDatabase($"TicketManagementSpecFlowTests-{Guid.NewGuid()}"));
            services.AddScoped<ITicketDbContext>(provider => provider.GetRequiredService<TicketDbContext>());
            services.AddSingleton<IEventCatalogClient>(EventCatalog);
        });

        builder.UseEnvironment("Testing");
    }
}
