using FluentValidation;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rps.Api.Http.Shared.Middleware;

/// <summary>
/// Global exception handler that maps well-known exceptions to structured ProblemDetails HTTP responses.
/// Replaces the old reflection-based validation failure path in <see cref="ValidationBehavior{TRequest,TResponse}"/>.
/// </summary>
internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case ValidationException validationEx:
                await HandleValidationExceptionAsync(httpContext, validationEx, cancellationToken);
                return true;

            default:
                logger.LogError(exception, "Unhandled exception occurred");
                await HandleUnexpectedExceptionAsync(httpContext, cancellationToken);
                return true;
        }
    }

    private static async Task HandleValidationExceptionAsync(
        HttpContext httpContext,
        ValidationException validationEx,
        CancellationToken cancellationToken)
    {
        var errors = validationEx.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

        var problemDetails = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation",
            Detail = "One or more validation failures have occurred.",
            Extensions = { ["errorCode"] = "VALIDATION_ERROR" }
        };

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
    }

    private static async Task HandleUnexpectedExceptionAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Unexpected",
            Detail = "An unexpected error occurred.",
            Extensions = { ["errorCode"] = "UNEXPECTED_ERROR" }
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
    }
}
