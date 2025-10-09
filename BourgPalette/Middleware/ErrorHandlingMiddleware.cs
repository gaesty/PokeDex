using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using BourgPalette.Errors;

namespace BourgPalette.Middleware;

public class ErrorHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (status, title) = ex switch
        {
            NotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
            ConflictException => (HttpStatusCode.Conflict, "Conflict"),
            BadRequestAppException => (HttpStatusCode.BadRequest, "Bad request"),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        // Log with appropriate level
        if (status == HttpStatusCode.InternalServerError)
            _logger.LogError(ex, "{Title}: {Message}", title, ex.Message);
        else
            _logger.LogWarning(ex, "{Title}: {Message}", title, ex.Message);

        var problem = new ProblemDetails
        {
            Status = (int)status,
            Title = title,
            Detail = ex.Message,
            Instance = context.TraceIdentifier,
            Type = "about:blank"
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problem.Status ?? (int)status;
        await context.Response.WriteAsJsonAsync(problem);
    }
}
