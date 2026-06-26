using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ripple.EventManagement.Application.Abstractions;
using Ripple.EventManagement.Infrastructure.Persistence;

namespace Ripple.EventManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddEventInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EventDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("EventDb")));
        services.AddScoped<IEventDbContext>(provider => provider.GetRequiredService<EventDbContext>());
        return services;
    }
}
