using ConfigMgrWebService.Shared.Constants;
using System.Diagnostics;

namespace ConfigMgrWebService.Api.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses with correlation IDs
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Generate or retrieve correlation ID
        var correlationId = context.Request.Headers[ApiConstants.Headers.CorrelationId].FirstOrDefault()
                           ?? Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers.Add(ApiConstants.Headers.CorrelationId, correlationId);

        // Log request
        var userName = context.User?.Identity?.Name ?? "Anonymous";
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        _logger.LogInformation(
            "HTTP {Method} {Path} started. CorrelationId: {CorrelationId}, User: {User}, IP: {ClientIp}",
            context.Request.Method,
            context.Request.Path,
            correlationId,
            userName,
            clientIp);

        var sw = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();

            var logLevel = context.Response.StatusCode >= 500 ? LogLevel.Error :
                          context.Response.StatusCode >= 400 ? LogLevel.Warning :
                          LogLevel.Information;

            _logger.Log(logLevel,
                "HTTP {Method} {Path} completed with {StatusCode} in {ElapsedMs}ms. CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds,
                correlationId);
        }
    }
}

/// <summary>
/// Extension method for registering the middleware
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
