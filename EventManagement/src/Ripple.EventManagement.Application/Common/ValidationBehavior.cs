using FluentValidation;
using MediatR;

namespace Ripple.EventManagement.Application.Common;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        //TODO - Refine
        if (!validators.Any()) return await next(cancellationToken);
        var context = new ValidationContext<TRequest>(request);
        var failures = validators.Select(x => x.Validate(context)).SelectMany(x => x.Errors).Where(x => x is not null).ToList();
        if (failures.Count != 0) throw new ValidationException(failures);
        return await next(cancellationToken);
    }
}
