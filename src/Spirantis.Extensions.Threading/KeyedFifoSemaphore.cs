namespace Spirantis.Extensions.Threading;

/// <summary>
/// Provides per-key, FIFO-ordered mutual exclusion.
/// </summary>
/// <remarks>
/// Each key is backed by its own single-slot <see cref="FifoSemaphore"/>, created
/// on first use and reference-counted so it is removed once the last holder
/// releases it. Callers waiting on the same key are granted the lock in the order
/// they called <see cref="WaitAsync"/>.
/// </remarks>
public sealed class KeyedFifoSemaphore
{
    private readonly Dictionary<string, ReferenceCounter<FifoSemaphore>> semaphoreCounters = [];
    private readonly Lock locker = new();

    /// <summary>
    /// Raised with the key when its last holder releases it and the key is removed.
    /// </summary>
    public event EventHandler<string>? LastProcessForKey;

    /// <summary>
    /// Gets a value indicating whether no keys are currently held.
    /// </summary>
    public bool IsEmpty
    {
        get
        {
            using (locker.EnterScope())
            {
                return semaphoreCounters.Count == 0;
            }
        }
    }

    /// <summary>
    /// Returns whether the given key is currently held or awaited by at least
    /// one caller.
    /// </summary>
    /// <param name="key">The key to test.</param>
    public bool ContainsKey(string key)
    {
        using (locker.EnterScope())
        {
            return semaphoreCounters.ContainsKey(key);
        }
    }

    /// <summary>
    /// Asynchronously acquires the lock for the given key.
    /// </summary>
    /// <param name="key">The key to lock.</param>
    /// <returns>
    /// An <see cref="IDisposable"/> that releases the key when disposed. Disposing
    /// it more than once is safe and has no additional effect.
    /// </returns>
    public async Task<IDisposable> WaitAsync(string key)
    {
        await GetOrCreate(key).WaitAsync().ConfigureAwait(false);

        var releaser = new KeyedFifoSemaphoreReleaser(key, semaphoreCounters, locker);

        releaser.LastProcess += (s, a) => LastProcessForKey?.Invoke(this, key);

        return releaser;
    }

    /// <summary>
    /// Returns the semaphore for the key, creating it if necessary, and increments
    /// its reference count under the shared lock.
    /// </summary>
    private FifoSemaphore GetOrCreate(string key)
    {
        ReferenceCounter<FifoSemaphore> semaphoreCounter;

        using (locker.EnterScope())
        {
            if (semaphoreCounters.TryGetValue(key, out var existing))
            {
                ++existing.Count;
                semaphoreCounter = existing;
            }
            else
            {
                semaphoreCounter = new ReferenceCounter<FifoSemaphore>(new FifoSemaphore(1, 1));
                semaphoreCounters[key] = semaphoreCounter;
            }
        }

        return semaphoreCounter.Value;
    }
}
