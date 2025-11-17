using ConfigMgrWebService.Shared.Constants;
using ConfigMgrWebService.Shared.Responses;
using System.Net;
using System.Text.Json;

namespace ConfigMgrWebService.Api.Middleware;

/// <summary>
/// Global exception handling middleware
/// Catches all unhandled exceptions and returns standardized API responses
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString();

        _logger.LogError(exception,
            "Unhandled exception occurred. CorrelationId: {CorrelationId}, Path: {Path}",
            correlationId, context.Request.Path);

        var response = ApiResponse.FailureResponse(
            exception.Message,
            ApiConstants.ResponseMessages.InternalServerError);

        response.CorrelationId = correlationId;

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        // Determine specific status code based on exception type
        context.Response.StatusCode = exception switch
        {
            ArgumentException => (int)HttpStatusCode.BadRequest,
            ArgumentNullException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(response, options);
        await context.Response.WriteAsync(json);
    }
}

/// <summary>
/// Extension method for registering the middleware
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
