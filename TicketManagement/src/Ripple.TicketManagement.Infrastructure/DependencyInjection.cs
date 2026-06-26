using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ripple.TicketManagement.Application.Abstractions;
using Ripple.TicketManagement.Infrastructure.Clients;
using Ripple.TicketManagement.Infrastructure.Persistence;

namespace Ripple.TicketManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTicketInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddDbContext<TicketDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("TicketDb")));
        services.AddScoped<ITicketDbContext>(provider => provider.GetRequiredService<TicketDbContext>());
        services.AddHttpClient<IEventCatalogClient, EventCatalogClient>(client => client.BaseAddress = new Uri(configuration["EventManagement:BaseUrl"]!));
        return services;
    }
}
