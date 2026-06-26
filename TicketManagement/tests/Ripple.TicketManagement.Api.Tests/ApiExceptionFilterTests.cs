using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Ripple.TicketManagement.Api.Tests;

public sealed class ApiExceptionFilterTests
{
    [Fact]
    public void OnException_ShouldReturnBadRequest_ForValidationException()
    {
        var context = CreateContext(new ValidationException(new[] { new ValidationFailure("Email", "Email is invalid.") }));

        new ApiExceptionFilter(new TestEnvironment { EnvironmentName = Environments.Production }, NullLogger<ApiExceptionFilter>.Instance).OnException(context);

        var result = context.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var problem = result.Value.Should().BeOfType<ValidationProblemDetails>().Subject;
        problem.Errors["Email"].Should().Contain("Email is invalid.");
    }

    [Fact]
    public void OnException_ShouldReturnBadRequest_ForInvalidOperationException()
    {
        var context = CreateContext(new InvalidOperationException("Business rule failed."));

        new ApiExceptionFilter(new TestEnvironment { EnvironmentName = Environments.Production }, NullLogger<ApiExceptionFilter>.Instance).OnException(context);

        var result = context.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        result.Value.Should().BeEquivalentTo(new ProblemDetails { Title = "Business rule failed.", Status = StatusCodes.Status400BadRequest });
    }

    [Fact]
    public void OnException_ShouldHideUnexpectedExceptionDetailOutsideDevelopment()
    {
        var context = CreateContext(new Exception("Sensitive details"));

        new ApiExceptionFilter(new TestEnvironment { EnvironmentName = Environments.Production }, NullLogger<ApiExceptionFilter>.Instance).OnException(context);

        var result = context.Result.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        var problem = result.Value.Should().BeOfType<ProblemDetails>().Subject;
        problem.Title.Should().Be("An unexpected error occurred.");
        problem.Detail.Should().BeNull();
    }

    [Fact]
    public void OnException_ShouldIncludeUnexpectedExceptionDetailInDevelopment()
    {
        var context = CreateContext(new Exception("Developer details"));

        new ApiExceptionFilter(new TestEnvironment { EnvironmentName = Environments.Development }, NullLogger<ApiExceptionFilter>.Instance).OnException(context);

        var problem = context.Result.Should().BeOfType<ObjectResult>().Subject.Value.Should().BeOfType<ProblemDetails>().Subject;
        problem.Detail.Should().Be("Developer details");
    }

    private static ExceptionContext CreateContext(Exception exception) => new(
        new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
        new List<IFilterMetadata>())
    { Exception = exception };

    private sealed class TestEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "Tests";
        public string WebRootPath { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
