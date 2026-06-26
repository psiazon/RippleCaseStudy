using FluentValidation;
using MediatR;

namespace Ripple.TicketManagement.Application.Common;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any()) return await next(cancellationToken);
        var failures = validators.Select(v => v.Validate(new ValidationContext<TRequest>(request))).SelectMany(r => r.Errors).Where(f => f is not null).ToList();
        if (failures.Count != 0) throw new ValidationException(failures);
        return await next(cancellationToken);
    }
}
