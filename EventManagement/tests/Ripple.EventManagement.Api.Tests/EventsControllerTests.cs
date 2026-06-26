using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Ripple.EventManagement.Api.Controllers;
using Ripple.EventManagement.Application.Events;
using Xunit;

namespace Ripple.EventManagement.Api.Tests;

public sealed class EventsControllerTests
{
    [Fact]
    public async Task Get_Should_send_query_and_return_ok_with_events()
    {
        var events = new List<EventDto> { SampleEventDto() };
        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.IsAny<GetEventsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(events);
        var controller = new EventsController(sender.Object);

        var result = await controller.Get(CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(events);
        sender.Verify(s => s.Send(It.IsAny<GetEventsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetById_Should_return_ok_when_event_exists()
    {
        var dto = SampleEventDto();
        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.Is<GetEventByIdQuery>(q => q.Id == dto.Id), It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var controller = new EventsController(sender.Object);

        var result = await controller.GetById(dto.Id, CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(dto);
    }

    [Fact]
    public async Task GetById_Should_return_not_found_when_event_is_missing()
    {
        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.IsAny<GetEventByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync((EventDto?)null);
        var controller = new EventsController(sender.Object);

        var result = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_Should_send_command_and_return_created_at_get_by_id()
    {
        var command = SampleCreateCommand();
        var created = SampleEventDto();
        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(command, It.IsAny<CancellationToken>())).ReturnsAsync(created);
        var controller = new EventsController(sender.Object);

        var result = await controller.Create(command, CancellationToken.None);

        var createdAt = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdAt.ActionName.Should().Be(nameof(EventsController.GetById));
        createdAt.Value.Should().BeSameAs(created);
        createdAt.RouteValues!["id"].Should().Be(created.Id);
    }

    [Fact]
    public async Task Update_Should_send_route_id_and_return_no_content_when_updated()
    {
        var routeId = Guid.NewGuid();
        var command = SampleUpdateCommand(Guid.NewGuid());
        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.Is<UpdateEventCommand>(c => c.Id == routeId), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var controller = new EventsController(sender.Object);

        var result = await controller.Update(routeId, command, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Update_Should_return_not_found_when_missing()
    {
        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.IsAny<UpdateEventCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var controller = new EventsController(sender.Object);

        var result = await controller.Update(Guid.NewGuid(), SampleUpdateCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_Should_return_no_content_when_deleted()
    {
        var id = Guid.NewGuid();
        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.Is<DeleteEventCommand>(c => c.Id == id), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var controller = new EventsController(sender.Object);

        var result = await controller.Delete(id, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_Should_return_not_found_when_missing()
    {
        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.IsAny<DeleteEventCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var controller = new EventsController(sender.Object);

        var result = await controller.Delete(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    private static EventDto SampleEventDto() => new(Guid.NewGuid(), "Name", "Description", "Venue", DateTimeOffset.UtcNow.AddDays(1), new TimeOnly(19, 0), 100, [new PricingTierDto(Guid.NewGuid(), "General", 10)]);

    private static CreateEventCommand SampleCreateCommand() => new("Name", "Description", "Venue", DateTimeOffset.UtcNow.AddDays(1), new TimeOnly(19, 0), 100, [new PricingTierRequest("General", 10)]);

    private static UpdateEventCommand SampleUpdateCommand(Guid id) => new(id, "Name", "Description", "Venue", DateTimeOffset.UtcNow.AddDays(1), new TimeOnly(19, 0), 100, [new PricingTierRequest("General", 10)]);
}
