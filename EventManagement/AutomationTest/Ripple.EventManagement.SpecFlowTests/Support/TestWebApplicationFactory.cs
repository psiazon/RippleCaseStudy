using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ripple.EventManagement.Application.Abstractions;
using Ripple.EventManagement.Infrastructure.Persistence;

namespace Ripple.EventManagement.SpecFlowTests.Support;

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"EventManagementSpecFlow_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = TestJwtTokenFactory.Issuer,
                ["Jwt:Audience"] = TestJwtTokenFactory.Audience,
                ["Jwt:SigningKey"] = TestJwtTokenFactory.SigningKey,
                ["Cors:AllowedOrigins:0"] = "https://localhost"
            };

            config.AddInMemoryCollection(settings);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<EventDbContext>>();
            services.RemoveAll<EventDbContext>();
            services.RemoveAll<IEventDbContext>();
            services.RemoveAll<DbConnection>();

            services.AddDbContext<EventDbContext>(options => options.UseInMemoryDatabase(_databaseName));
            services.AddScoped<IEventDbContext>(provider => provider.GetRequiredService<EventDbContext>());

            services.AddHttpClient("TicketInventory")
                .ConfigurePrimaryHttpMessageHandler(() => new SuccessfulTicketInventoryHandler());

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        });
    }
}
