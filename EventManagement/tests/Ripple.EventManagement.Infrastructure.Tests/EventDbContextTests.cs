using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ripple.EventManagement.Application.Abstractions;
using Ripple.EventManagement.Domain.Entities;
using Ripple.EventManagement.Infrastructure;
using Ripple.EventManagement.Infrastructure.Persistence;
using Xunit;

namespace Ripple.EventManagement.Infrastructure.Tests;

public sealed class EventDbContextTests
{
    [Fact]
    public void DbSets_Should_return_entity_sets()
    {
        using var db = CreateContext();

        db.Events.Should().NotBeNull();
        db.PricingTiers.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveChanges_Should_persist_event_and_pricing_tiers_with_inmemory_provider()
    {
        await using var db = CreateContext();
        var evt = new Event("Name", "Description", "Venue", DateTimeOffset.UtcNow.AddDays(1), TimeOnly.MinValue, 10, [new PricingTier("General", 10)]);

        db.Events.Add(evt);
        await db.SaveChangesAsync(CancellationToken.None);

        var saved = await db.Events.Include(e => e.PricingTiers).SingleAsync(e => e.Id == evt.Id);        
    }

    [Fact]
    public void OnModelCreating_Should_configure_expected_tables_keys_lengths_and_decimal_type()
    {
        using var db = CreateContext();
        var model = db.Model;
        var eventEntity = model.FindEntityType(typeof(Event));
        var tierEntity = model.FindEntityType(typeof(PricingTier));

        eventEntity.Should().NotBeNull();
        eventEntity!.GetTableName().Should().Be("Events");
        eventEntity.FindPrimaryKey()!.Properties.Should().ContainSingle(p => p.Name == nameof(Event.Id));
        eventEntity.FindProperty(nameof(Event.Name))!.GetMaxLength().Should().Be(200);
        eventEntity.FindProperty(nameof(Event.Description))!.GetMaxLength().Should().Be(2000);
        eventEntity.FindProperty(nameof(Event.Venue))!.GetMaxLength().Should().Be(300);

        tierEntity.Should().NotBeNull();
        tierEntity!.GetTableName().Should().Be("PricingTiers");
        tierEntity.FindPrimaryKey()!.Properties.Should().ContainSingle(p => p.Name == nameof(PricingTier.Id));
        tierEntity.FindProperty(nameof(PricingTier.Name))!.GetMaxLength().Should().Be(100);        
    }

    [Fact]
    public void AddEventInfrastructure_Should_register_context_and_abstraction()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:EventDb"] = "Server=(localdb)\\MSSQLLocalDB;Database=RippleEventTests;Trusted_Connection=True;" })
            .Build();
        var services = new ServiceCollection();

        services.AddEventInfrastructure(configuration);

        var descriptors = services.Select(s => (s.ServiceType, s.ImplementationType, s.Lifetime)).ToList();
        descriptors.Should().Contain(d => d.ServiceType == typeof(EventDbContext));
        descriptors.Should().Contain(d => d.ServiceType == typeof(IEventDbContext) && d.Lifetime == ServiceLifetime.Scoped);
    }

    private static EventDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<EventDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new EventDbContext(options);
    }
}
