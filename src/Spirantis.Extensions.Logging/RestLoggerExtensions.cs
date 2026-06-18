namespace Microsoft.Extensions.Logging;

/// <summary>
/// Structured logging helpers for HTTP request/response activity. Each value is captured
/// as a named structured property (Method, Path, StatusCode, …) so it stays queryable,
/// rather than being interpolated into the message text.
/// </summary>
public static class RestLoggerExtensions
{
    /// <summary>Logs an incoming HTTP request.</summary>
    public static void LogIncomingRequest(
        this ILogger logger,
        string method,
        string path,
        string? correlationKey = null,
        LogLevel level = LogLevel.Information
    ) =>
        logger.Log(
            level,
            "Incoming {Method} request to {Path} {CorrelationKey}",
            method,
            path,
            correlationKey
        );

    /// <summary>Logs the response to an incoming HTTP request.</summary>
    public static void LogIncomingResponse(
        this ILogger logger,
        string method,
        string path,
        int? statusCode,
        double elapsedMilliseconds,
        int? retryCount = null,
        string? correlationKey = null,
        LogLevel level = LogLevel.Information
    ) =>
        logger.Log(
            level,
            "Incoming {Method} response for {Path}: status {StatusCode} after {ElapsedMilliseconds} ms; retries {RetryCount} {CorrelationKey}",
            method,
            path,
            statusCode,
            elapsedMilliseconds,
            retryCount,
            correlationKey
        );

    /// <summary>Logs an outgoing HTTP request.</summary>
    public static void LogOutgoingRequest(
        this ILogger logger,
        string method,
        string path,
        string? correlationKey = null,
        LogLevel level = LogLevel.Information
    ) =>
        logger.Log(
            level,
            "Outgoing {Method} request to {Path} {CorrelationKey}",
            method,
            path,
            correlationKey
        );

    /// <summary>Logs the response to an outgoing HTTP request.</summary>
    public static void LogOutgoingResponse(
        this ILogger logger,
        string method,
        string path,
        int? statusCode,
        double elapsedMilliseconds,
        int? retryCount = null,
        string? correlationKey = null,
        LogLevel level = LogLevel.Information
    ) =>
        logger.Log(
            level,
            "Outgoing {Method} response for {Path}: status {StatusCode} after {ElapsedMilliseconds} ms; retries {RetryCount} {CorrelationKey}",
            method,
            path,
            statusCode,
            elapsedMilliseconds,
            retryCount,
            correlationKey
        );
}
