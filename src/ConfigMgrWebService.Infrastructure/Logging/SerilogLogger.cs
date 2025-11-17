using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Context;
using System.Runtime.CompilerServices;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace ConfigMgrWebService.Infrastructure.Logging;

/// <summary>
/// Structured logging wrapper using Serilog
/// Maintains compatibility with legacy logging format while adding modern structured logging
/// </summary>
public class StructuredLogger
{
    private readonly ILogger _logger;
    private readonly string _sourceName;

    public StructuredLogger(ILogger logger, string sourceName)
    {
        _logger = logger;
        _sourceName = sourceName;
    }

    /// <summary>
    /// Writes a log message with severity, correlation, and context
    /// </summary>
    public void WriteMessage(
        LogLevel logLevel,
        string message,
        string? correlationId = null,
        string? userName = null,
        string? clientName = null,
        [CallerMemberName] string methodName = "",
        [CallerFilePath] string? callingFilePath = null,
        [CallerLineNumber] int callerLineNumber = 0)
    {
        using (LogContext.PushProperty("SourceName", _sourceName))
        using (LogContext.PushProperty("MethodName", methodName))
        using (LogContext.PushProperty("ThreadId", Environment.CurrentManagedThreadId))
        using (LogContext.PushProperty("FilePath", callingFilePath))
        using (LogContext.PushProperty("LineNumber", callerLineNumber))
        {
            if (!string.IsNullOrEmpty(correlationId))
            {
                using (LogContext.PushProperty("CorrelationId", correlationId))
                {
                    LogWithContext(logLevel, message, userName, clientName);
                }
            }
            else
            {
                LogWithContext(logLevel, message, userName, clientName);
            }
        }
    }

    private void LogWithContext(LogLevel logLevel, string message, string? userName, string? clientName)
    {
        var contextMessage = message;

        if (!string.IsNullOrEmpty(clientName) || !string.IsNullOrEmpty(userName))
        {
            var prefix = string.IsNullOrEmpty(userName)
                ? $"Request from computer \"{clientName}\""
                : $"Request from user \"{userName}\" on computer \"{clientName}\"";

            contextMessage = $"{prefix}: {message}";
        }

        _logger.Log(logLevel, contextMessage);
    }

    /// <summary>
    /// Logs an error with exception details
    /// </summary>
    public void LogError(
        Exception exception,
        string message,
        string? correlationId = null,
        [CallerMemberName] string methodName = "")
    {
        using (LogContext.PushProperty("SourceName", _sourceName))
        using (LogContext.PushProperty("MethodName", methodName))
        using (LogContext.PushProperty("ThreadId", Environment.CurrentManagedThreadId))
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogError(exception, message);
        }
    }

    /// <summary>
    /// Logs API request information
    /// </summary>
    public void LogApiRequest(
        string method,
        string path,
        string? correlationId,
        string? userName,
        string? clientIp)
    {
        using (LogContext.PushProperty("SourceName", _sourceName))
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("HttpMethod", method))
        using (LogContext.PushProperty("RequestPath", path))
        using (LogContext.PushProperty("UserName", userName))
        using (LogContext.PushProperty("ClientIp", clientIp))
        {
            _logger.LogInformation("API Request: {HttpMethod} {RequestPath} from {UserName} ({ClientIp})",
                method, path, userName ?? "Anonymous", clientIp ?? "Unknown");
        }
    }

    /// <summary>
    /// Logs API response information
    /// </summary>
    public void LogApiResponse(
        string method,
        string path,
        int statusCode,
        long elapsedMs,
        string? correlationId)
    {
        using (LogContext.PushProperty("SourceName", _sourceName))
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("HttpMethod", method))
        using (LogContext.PushProperty("RequestPath", path))
        using (LogContext.PushProperty("StatusCode", statusCode))
        using (LogContext.PushProperty("ElapsedMs", elapsedMs))
        {
            var logLevel = statusCode >= 500 ? LogLevel.Error :
                          statusCode >= 400 ? LogLevel.Warning :
                          LogLevel.Information;

            _logger.Log(logLevel, "API Response: {HttpMethod} {RequestPath} returned {StatusCode} in {ElapsedMs}ms",
                method, path, statusCode, elapsedMs);
        }
    }
}
