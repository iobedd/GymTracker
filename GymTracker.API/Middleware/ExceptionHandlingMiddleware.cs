using System.Net;
using System.Text.Json;

namespace GymTracker.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "Eroare neasteptata la {Path}", context.Request.Path);

            var (status, title) = ex switch
            {
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Neautorizat"),
                KeyNotFoundException => (HttpStatusCode.NotFound, "Resursa nu a fost gasita"),
                ArgumentException => (HttpStatusCode.BadRequest, "Cerere invalida"),
                InvalidOperationException => (HttpStatusCode.Conflict, ex.Message),
                _ => (HttpStatusCode.InternalServerError, "A aparut o eroare interna")
            };

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)status;

            var problem = new
            {
                type = $"https://httpstatuses.io/{(int)status}",
                title,
                status = (int)status,
                detail = status == HttpStatusCode.InternalServerError ? null : ex.Message,
                traceId = context.TraceIdentifier
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}
