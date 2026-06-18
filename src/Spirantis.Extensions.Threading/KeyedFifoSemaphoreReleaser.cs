namespace Spirantis.Extensions.Threading;

/// <summary>
/// The disposable handle returned by <see cref="KeyedFifoSemaphore.WaitAsync"/>.
/// Disposing it releases the key and, when it is the last holder, removes and
/// disposes the underlying semaphore.
/// </summary>
internal sealed class KeyedFifoSemaphoreReleaser(
    string key,
    Dictionary<string, ReferenceCounter<FifoSemaphore>> semaphoreCounters,
    Lock locker
) : IDisposable
{
    private bool disposed;

    /// <summary>
    /// Raised when disposing this releaser removes the key (i.e. it was the last
    /// holder).
    /// </summary>
    public event EventHandler? LastProcess;

    /// <summary>
    /// Releases the key. Safe to call multiple times; only the first call has an
    /// effect.
    /// </summary>
    public void Dispose()
    {
        ReferenceCounter<FifoSemaphore> semaphoreCounter;
        bool disposeSemaphore = false;

        using (locker.EnterScope())
        {
            if (disposed)
            {
                return;
            }

            disposed = true;

            semaphoreCounter = semaphoreCounters[key];

            --semaphoreCounter.Count;

            if (semaphoreCounter.Count == 0)
            {
                semaphoreCounters.Remove(key);
                LastProcess?.Invoke(this, EventArgs.Empty);
                disposeSemaphore = true;
            }
        }

        semaphoreCounter.Value.Release();

        if (disposeSemaphore)
        {
            semaphoreCounter.Value.Dispose();
        }
    }
}
