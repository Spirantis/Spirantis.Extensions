using Microsoft.Extensions.Logging;

namespace Spirantis.Extensions.Logging.Tests;

/// <summary>An <see cref="ILogger"/> that records every log call for assertions.</summary>
internal sealed class CapturingLogger : ILogger
{
    public List<LogEntry> Entries { get; } = [];

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        var properties =
            state as IReadOnlyList<KeyValuePair<string, object?>>
            ?? Array.Empty<KeyValuePair<string, object?>>();

        Entries.Add(new LogEntry(logLevel, formatter(state, exception), exception, properties));
    }

    internal sealed record LogEntry(
        LogLevel Level,
        string Message,
        Exception? Exception,
        IReadOnlyList<KeyValuePair<string, object?>> Properties
    )
    {
        public object? Property(string name) =>
            Properties.FirstOrDefault(pair => pair.Key == name).Value;
    }
}
