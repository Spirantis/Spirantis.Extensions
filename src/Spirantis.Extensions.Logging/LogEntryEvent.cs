namespace Spirantis.Extensions.Logging;

/// <summary>
/// Base type for structured log events. Carries a correlation <see cref="Key"/>, a
/// classification (<see cref="EventType"/>), a free-text <see cref="Message"/>, and a
/// <see cref="EventName"/> derived from the concrete type. Intended to be logged as a
/// structured object via the event-logging extensions.
/// </summary>
/// <typeparam name="TKey">The type of the correlation key.</typeparam>
public abstract class LogEntryEvent<TKey>
{
    /// <summary>Creates an internal event with the given key.</summary>
    protected LogEntryEvent(TKey key) => Key = key;

    /// <summary>Creates an internal event with the given key and message.</summary>
    protected LogEntryEvent(TKey key, string message)
        : this(key) => Message = message;

    /// <summary>Creates an event with the given key and classification.</summary>
    protected LogEntryEvent(TKey key, LogEntryEventType eventType)
        : this(key) => EventType = eventType;

    /// <summary>Creates an event with the given key, classification, and message.</summary>
    protected LogEntryEvent(TKey key, LogEntryEventType eventType, string message)
        : this(key, message) => EventType = eventType;

    /// <summary>The correlation key for the event.</summary>
    public TKey Key { get; protected set; }

    /// <summary>The event classification. Defaults to <see cref="LogEntryEventType.Internal"/>.</summary>
    public LogEntryEventType EventType { get; private set; } = LogEntryEventType.Internal;

    /// <summary>A free-text message describing the event.</summary>
    public string Message { get; protected set; } = string.Empty;

    /// <summary>The concrete event type name (open-generic and array names simplified).</summary>
    public string EventName => CleanTypeName(GetType());

    private static string CleanTypeName(Type type)
    {
        // GetType() on an event instance is always a (non-array) class, so only the
        // open-generic arity suffix needs trimming.
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        string name = type.GetGenericTypeDefinition().Name;
        int index = name.IndexOf('`', StringComparison.Ordinal);

        return index < 0 ? name : name[..index];
    }
}
