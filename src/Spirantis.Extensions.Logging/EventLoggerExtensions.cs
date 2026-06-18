using Microsoft.Extensions.Logging;

namespace Spirantis.Extensions.Logging;

/// <summary>
/// Logs <see cref="LogEntryEvent{TKey}"/> instances as structured log events, so the
/// event's properties are captured for querying rather than flattened into the message.
/// </summary>
public static class EventLoggerExtensions
{
    /// <summary>
    /// The message template used to log events. The event is captured under the
    /// <c>Event</c> structured property.
    /// </summary>
    public const string EventMessageTemplate = "{@Event}";

    /// <summary>Logs <paramref name="logEvent"/> at <see cref="LogLevel.Trace"/>.</summary>
    public static void LogTrace<TKey>(this ILogger logger, LogEntryEvent<TKey> logEvent) =>
        logger.LogTrace(EventMessageTemplate, logEvent);

    /// <summary>Logs <paramref name="logEvent"/> at <see cref="LogLevel.Debug"/>.</summary>
    public static void LogDebug<TKey>(this ILogger logger, LogEntryEvent<TKey> logEvent) =>
        logger.LogDebug(EventMessageTemplate, logEvent);

    /// <summary>Logs <paramref name="logEvent"/> at <see cref="LogLevel.Information"/>.</summary>
    public static void LogInformation<TKey>(this ILogger logger, LogEntryEvent<TKey> logEvent) =>
        logger.LogInformation(EventMessageTemplate, logEvent);

    /// <summary>Logs <paramref name="logEvent"/> at <see cref="LogLevel.Warning"/>.</summary>
    public static void LogWarning<TKey>(this ILogger logger, LogEntryEvent<TKey> logEvent) =>
        logger.LogWarning(EventMessageTemplate, logEvent);

    /// <summary>Logs <paramref name="logEvent"/> at <see cref="LogLevel.Error"/>.</summary>
    public static void LogError<TKey>(this ILogger logger, LogEntryEvent<TKey> logEvent) =>
        logger.LogError(EventMessageTemplate, logEvent);

    /// <summary>Logs <paramref name="logEvent"/> with an exception at <see cref="LogLevel.Error"/>.</summary>
    public static void LogError<TKey>(
        this ILogger logger,
        Exception exception,
        LogEntryEvent<TKey> logEvent
    ) => logger.LogError(exception, EventMessageTemplate, logEvent);

    /// <summary>Logs <paramref name="logEvent"/> at <see cref="LogLevel.Critical"/>.</summary>
    public static void LogCritical<TKey>(this ILogger logger, LogEntryEvent<TKey> logEvent) =>
        logger.LogCritical(EventMessageTemplate, logEvent);

    /// <summary>Logs <paramref name="logEvent"/> with an exception at <see cref="LogLevel.Critical"/>.</summary>
    public static void LogCritical<TKey>(
        this ILogger logger,
        Exception exception,
        LogEntryEvent<TKey> logEvent
    ) => logger.LogCritical(exception, EventMessageTemplate, logEvent);
}
