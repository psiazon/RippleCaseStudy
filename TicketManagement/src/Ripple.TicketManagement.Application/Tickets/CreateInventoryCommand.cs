using FluentValidation;
using MediatR;
using Ripple.TicketManagement.Application.Abstractions;
using Ripple.TicketManagement.Domain.Entities;

namespace Ripple.TicketManagement.Application.Tickets;

public sealed record CreateInventoryCommand(Guid EventId, int TotalCapacity) : IRequest<Unit>;

public sealed class CreateInventoryValidator : AbstractValidator<CreateInventoryCommand>
{
    public CreateInventoryValidator()
    {
        RuleFor(x => x.EventId).NotEmpty();
        RuleFor(x => x.TotalCapacity).GreaterThan(0);
    }
}

public sealed class CreateInventoryHandler(ITicketDbContext db) : IRequestHandler<CreateInventoryCommand, Unit>
{
    public async Task<Unit> Handle(CreateInventoryCommand request, CancellationToken cancellationToken)
    {
        var existing = await db.Inventories.FindAsync(new object[] { request.EventId }, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException("An inventory record for the specified event already exists.");

        var inventory = new TicketInventory(request.EventId, request.TotalCapacity);
        db.Inventories.Add(inventory);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
