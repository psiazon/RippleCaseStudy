using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ripple.TicketManagement.Api.Controllers;
using Ripple.TicketManagement.Application.Tickets;
using Xunit;

namespace Ripple.TicketManagement.Api.Tests;

public sealed class TicketsControllerTests
{
    [Fact]
    public async Task Purchase_ShouldSendCommandAndReturnOk()
    {
        var expected = new TicketOrderDto(Guid.NewGuid(), Guid.NewGuid(), "buyer@example.com", 2, 10m, 20m, DateTimeOffset.UtcNow);
        var sender = new CapturingSender(expected);
        var command = new PurchaseTicketsCommand(expected.EventId, Guid.NewGuid(), expected.PurchaserEmail, expected.Quantity);

        var result = await new TicketsController(sender).Purchase(command, CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(expected);
        sender.LastRequest.Should().Be(command);
    }

    [Fact]
    public async Task CreateInventory_ShouldSendCommandAndReturnCreatedAtAvailability()
    {
        var sender = new CapturingSender(Unit.Value);
        var command = new CreateInventoryCommand(Guid.NewGuid(), 100);

        var result = await new TicketsController(sender).CreateInventory(command, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(TicketsController.Availability));
        created.RouteValues!["eventId"].Should().Be(command.EventId);
        sender.LastRequest.Should().Be(command);
    }

    [Fact]
    public async Task Availability_ShouldSendQueryAndReturnOk()
    {
        var expected = new AvailabilityDto(Guid.NewGuid(), 10, 3, 7);
        var sender = new CapturingSender(expected);

        var result = await new TicketsController(sender).Availability(expected.EventId, CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>().Subject.Value.Should().Be(expected);
        sender.LastRequest.Should().Be(new GetAvailabilityQuery(expected.EventId));
    }

    [Fact]
    public async Task Sales_ShouldSendQueryAndReturnOk()
    {
        var expected = new EventSalesSummaryDto(Guid.NewGuid(), 2, 5, 100m);
        var sender = new CapturingSender(expected);

        var result = await new TicketsController(sender).Sales(expected.EventId, CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>().Subject.Value.Should().Be(expected);
        sender.LastRequest.Should().Be(new GetSalesSummaryQuery(expected.EventId));
    }

    private sealed class CapturingSender(object response) : ISender
    {
        public object? LastRequest { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult((TResponse)response);
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult<object?>(response);
        }

        public async IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }

        public async IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }

        Task ISender.Send<TRequest>(TRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

}
