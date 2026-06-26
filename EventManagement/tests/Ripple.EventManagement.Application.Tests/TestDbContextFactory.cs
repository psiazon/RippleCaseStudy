using Microsoft.EntityFrameworkCore;
using Ripple.EventManagement.Infrastructure.Persistence;

namespace Ripple.EventManagement.Application.Tests;

internal static class TestDbContextFactory
{
    public static EventDbContext Create()
    {
        var options = new DbContextOptionsBuilder<EventDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new EventDbContext(options);
    }
}
