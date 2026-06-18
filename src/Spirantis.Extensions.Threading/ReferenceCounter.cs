namespace Spirantis.Extensions.Threading;

/// <summary>
/// Pairs a value with a mutable reference count, used to keep a shared resource
/// alive while it has active users and remove it once the count reaches zero.
/// </summary>
/// <typeparam name="T">The type of the counted value.</typeparam>
/// <param name="value">The value to track. The count starts at one.</param>
internal sealed class ReferenceCounter<T>(T value)
{
    /// <summary>
    /// Gets or sets the number of active users of <see cref="Value"/>.
    /// </summary>
    public int Count { get; set; } = 1;

    /// <summary>
    /// Gets the counted value.
    /// </summary>
    public T Value { get; } = value;
}
