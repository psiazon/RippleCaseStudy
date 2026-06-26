using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Ripple.EventManagement.Api.Controllers;
using Ripple.EventManagement.Application.Events;
using Xunit;

namespace Ripple.EventManagement.Tests
{
    public class EventsControllerUnitTests
    {
        private static T CreateUninitialized<T>() where T : class
            => (T)FormatterServices.GetUninitializedObject(typeof(T));

        private static EventDto CreateEventDtoWithId()
        {
            var dto = CreateUninitialized<EventDto>();
            var idProp = typeof(EventDto).GetProperty("Id");
            if (idProp != null && idProp.CanWrite) idProp.SetValue(dto, Guid.NewGuid());
            return dto;
        }

        [Fact]
        public async Task Get_ReturnsOk()
        {
            // Arrange
            var mock = new Mock<ISender>();
            mock.Setup(s => s.Send(It.IsAny<GetEventsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EventDto>() {
                    new EventDto(
                        Guid.NewGuid(),
                        "Test Event",
                        "Test Description",
                        "Test Venue",
                        DateTimeOffset.UtcNow,
                        new TimeOnly(12, 0),
                        100,
                        Array.Empty<PricingTierDto>())
                });

            var controller = new EventsController(mock.Object);

            // Act
            var result = await controller.Get(CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            var events = Assert.IsAssignableFrom<IReadOnlyCollection<EventDto>>(okResult.Value);

            Assert.NotEmpty(events);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenFound()
        {
            // Arrange
            var dto = CreateEventDtoWithId();

            var mock = new Mock<ISender>();
            mock.Setup(s => s.Send(It.IsAny<GetEventByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var controller = new EventsController(mock.Object);

            // Act
            var result = await controller.GetById(dto.Id, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(dto, ok.Value);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            var mock = new Mock<ISender>();
            mock.Setup(s => s.Send(It.IsAny<GetEventByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((EventDto?)null);

            var controller = new EventsController(mock.Object);

            // Act
            var result = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

            // Assert: allow NotFoundResult or 404 ObjectResult
            if (result.Result is StatusCodeResult sc) Assert.Equal(404, sc.StatusCode);
            else if (result.Result is ObjectResult o) Assert.Equal(404, o.StatusCode);
            else Assert.Fail($"Expected 404 result, got {result?.Result?.GetType().Name}");
        }

        [Fact]
        public async Task Create_ReturnsCreatedAt()
        {
            // Arrange
            var created = CreateEventDtoWithId();

            var mock = new Mock<ISender>();
            mock.Setup(s => s.Send(It.IsAny<CreateEventCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(created);

            var controller = new EventsController(mock.Object);

            // Build a CreateEventCommand instance without calling constructor
            var createCmd = CreateUninitialized<CreateEventCommand>();

            // Act
            var result = await controller.Create(createCmd, CancellationToken.None);

            // Assert
            var createdAt = result.Result;
            Assert.Equal(nameof(EventsController.GetById), ((Microsoft.AspNetCore.Mvc.CreatedAtActionResult)result.Result).ActionName);
            //Assert.Same(created, createdAt.Value);
        }
        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WithCreatedDto()
        {
            var created = new EventDto(
                Guid.NewGuid(),
                "n",
                "d",
                "v",
                DateTime.UtcNow,
                TimeOnly.FromDateTime(DateTime.UtcNow),
                5,
                Array.Empty<PricingTierDto>()); // <-- Add this argument

            var sender = new Mock<ISender>();
            sender.Setup(s => s.Send(It.IsAny<CreateEventCommand>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(created);

            var controller = new EventsController(sender.Object);

            var createCmd = new CreateEventCommand("n", "d", "v", DateTimeOffset.MaxValue,
                                                   TimeOnly.FromDateTime(DateTime.UtcNow), 10, Array.Empty<PricingTierRequest>());

            var actionResult = await controller.Create(createCmd, CancellationToken.None);

            // Unwrap ActionResult<T> and assert the IActionResult is CreatedAtActionResult
            var createdAt = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            Assert.Equal(nameof(EventsController.GetById), createdAt.ActionName);
            Assert.Same(created, createdAt.Value);
            Assert.Equal(created.Id, ((dynamic)createdAt.RouteValues!)["id"]);
        }

        [Fact]
        public async Task Update_ReturnsNoContent_WhenUpdated()
        {
            // Arrange
            var mock = new Mock<ISender>();
            mock.Setup(s => s.Send(It.IsAny<UpdateEventCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var controller = new EventsController(mock.Object);

            var updateCmd = CreateUninitialized<UpdateEventCommand>();

            // Act
            var result = await controller.Update(Guid.NewGuid(), updateCmd, CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            var mock = new Mock<ISender>();
            mock.Setup(s => s.Send(It.IsAny<UpdateEventCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var controller = new EventsController(mock.Object);

            var updateCmd = CreateUninitialized<UpdateEventCommand>();

            // Act
            var result = await controller.Update(Guid.NewGuid(), updateCmd, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenDeleted()
        {
            // Arrange
            var mock = new Mock<ISender>();
            mock.Setup(s => s.Send(It.IsAny<DeleteEventCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var controller = new EventsController(mock.Object);

            // Act
            var result = await controller.Delete(Guid.NewGuid(), CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            var mock = new Mock<ISender>();
            mock.Setup(s => s.Send(It.IsAny<DeleteEventCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var controller = new EventsController(mock.Object);

            // Act
            var result = await controller.Delete(Guid.NewGuid(), CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}

