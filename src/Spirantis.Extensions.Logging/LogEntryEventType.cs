namespace Spirantis.Extensions.Logging;

/// <summary>Classifies the nature of a <see cref="LogEntryEvent{TKey}"/>.</summary>
public enum LogEntryEventType
{
    /// <summary>An event crossing a system boundary (external calls, messaging, I/O).</summary>
    Integration,

    /// <summary>An event with business/domain significance.</summary>
    Business,

    /// <summary>An internal, technical event. The default.</summary>
    Internal,
}
