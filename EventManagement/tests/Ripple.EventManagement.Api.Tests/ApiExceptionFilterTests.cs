using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Ripple.EventManagement.Api.Tests;

public sealed class ApiExceptionFilterTests
{
    [Fact]
    public void OnException_Should_return_bad_request_for_validation_exception()
    {
        var filter = new ApiExceptionFilter(MockEnvironment(Environments.Production), NullLogger<ApiExceptionFilter>.Instance);
        var context = CreateContext(new ValidationException([new ValidationFailure("Name", "Name is required"), new ValidationFailure("Name", "Name is too long")]));

        filter.OnException(context);

        var badRequest = context.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var details = badRequest.Value.Should().BeOfType<ValidationProblemDetails>().Subject;
        details.Errors.Should().ContainKey("Name").WhoseValue.Should().Contain(["Name is required", "Name is too long"]);
    }

    [Fact]
    public void OnException_Should_return_problem_details_without_exception_detail_outside_development()
    {
        var filter = new ApiExceptionFilter(MockEnvironment(Environments.Production), NullLogger<ApiExceptionFilter>.Instance);
        var context = CreateContext(new InvalidOperationException("Sensitive detail"));

        filter.OnException(context);

        var result = context.Result.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        var details = result.Value.Should().BeOfType<ProblemDetails>().Subject;
        details.Title.Should().Be("An unexpected error occurred.");
        details.Detail.Should().BeNull();
        details.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void OnException_Should_include_exception_message_in_development()
    {
        var filter = new ApiExceptionFilter(MockEnvironment(Environments.Development), NullLogger<ApiExceptionFilter>.Instance);
        var context = CreateContext(new InvalidOperationException("Developer detail"));

        filter.OnException(context);

        var result = context.Result.Should().BeOfType<ObjectResult>().Subject;
        var details = result.Value.Should().BeOfType<ProblemDetails>().Subject;
        details.Detail.Should().Be("Developer detail");
    }

    private static ExceptionContext CreateContext(Exception exception)
    {
        var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        return new ExceptionContext(actionContext, []) { Exception = exception };
    }

    private static IWebHostEnvironment MockEnvironment(string environmentName)
    {
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(e => e.EnvironmentName).Returns(environmentName);
        return environment.Object;
    }
}
