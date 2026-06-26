using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public sealed class ApiExceptionFilter(IHostEnvironment environment, ILogger<ApiExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is ValidationException validationException)
        {
            context.Result = new BadRequestObjectResult(new ValidationProblemDetails(validationException.Errors.GroupBy(x => x.PropertyName).ToDictionary(x => x.Key, x => x.Select(e => e.ErrorMessage).ToArray())));
            return;
        }
        if (context.Exception is InvalidOperationException invalidOperation)
        {
            context.Result = new BadRequestObjectResult(new ProblemDetails { Title = invalidOperation.Message, Status = StatusCodes.Status400BadRequest });
            return;
        }
        logger.LogError(context.Exception, "Unhandled exception");
        context.Result = new ObjectResult(new ProblemDetails { Title = "An unexpected error occurred.", Detail = environment.IsDevelopment() ? context.Exception.Message : null, Status = StatusCodes.Status500InternalServerError }) { StatusCode = 500 };
    }
}
