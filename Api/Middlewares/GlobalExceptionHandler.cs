using System;
using System.Threading;
using System.Threading.Tasks;
using JobApplication.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JobApplication.Api.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            NotFoundException notFound => (StatusCodes.Status404NotFound, "Not Found", notFound.Message),
            ForbiddenAccessException forbidden => (StatusCodes.Status403Forbidden, "Forbidden", forbidden.Message),
            UnauthorizedAccessException unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized", unauthorized.Message),
            BadRequestException badRequest => (StatusCodes.Status400BadRequest, "Bad Request", badRequest.Message),
            ArgumentException argEx => (StatusCodes.Status400BadRequest, "Invalid Argument", argEx.Message),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred. Please try again later.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred while processing request to '{Path}'.", httpContext.Request.Path);
        }
        else
        {
            _logger.LogWarning("Handled application exception ({StatusCode} {Title}): {Detail} at '{Path}'.",
                statusCode, title, detail, httpContext.Request.Path);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
