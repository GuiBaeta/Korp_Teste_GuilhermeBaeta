using Inventory.Api.DTOs;

namespace Inventory.Api.Exceptions;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            KeyNotFoundException => (
                StatusCodes.Status404NotFound,
                exception.Message),
            ArgumentException => (
                StatusCodes.Status400BadRequest,
                exception.Message),
            InvalidOperationException => (
                StatusCodes.Status409Conflict,
                exception.Message),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);
        }
        else
        {
            _logger.LogWarning(
                "Request failed with status {StatusCode}: {Message}",
                statusCode,
                message);
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new ApiErrorResponse
        {
            StatusCode = statusCode,
            Message = message,
            TraceId = context.TraceIdentifier
        });
    }
}
