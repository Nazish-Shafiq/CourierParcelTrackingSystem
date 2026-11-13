using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace CourierParcelTrackingSystem.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled API exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);

            var (status, message) = ex switch
            {
                ArgumentException => (HttpStatusCode.BadRequest, ex.Message),
                ValidationException => (HttpStatusCode.BadRequest, ex.Message),
                KeyNotFoundException => (HttpStatusCode.NotFound, ex.Message),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Authentication is required or the supplied credentials are invalid."),
                DbUpdateConcurrencyException => (HttpStatusCode.Conflict, "The record was changed by another operation."),
                DbUpdateException => (HttpStatusCode.Conflict, "The database rejected the operation. Check unique and foreign-key constraints."),
                _ => (HttpStatusCode.InternalServerError, "An unexpected server error occurred.")
            };

            context.Response.StatusCode = (int)status;
            context.Response.ContentType = "application/json";

            var payload = new
            {
                success = false,
                message,
                data = (object?)null
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
