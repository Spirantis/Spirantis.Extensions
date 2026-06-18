using System.Collections.Concurrent;

namespace Spirantis.Extensions.Threading;

/// <summary>
/// An asynchronous semaphore that grants entry to waiters in strict
/// first-in, first-out order.
/// </summary>
/// <remarks>
/// <see cref="SemaphoreSlim"/> does not guarantee the order in which queued
/// waiters are released. <see cref="FifoSemaphore"/> layers an ordered queue on
/// top of it so that callers are granted entry in the exact order they called
/// <see cref="WaitAsync"/>.
/// </remarks>
public sealed class FifoSemaphore : IDisposable
{
    private readonly ConcurrentQueue<TaskCompletionSource> queue = new();
    private readonly SemaphoreSlim semaphore;

    /// <summary>
    /// Initializes a new instance with the given number of slots available.
    /// </summary>
    /// <param name="initialCount">
    /// The initial number of concurrent entries permitted.
    /// </param>
    public FifoSemaphore(int initialCount)
    {
        semaphore = new SemaphoreSlim(initialCount);
    }

    /// <summary>
    /// Initializes a new instance with the given initial and maximum slot counts.
    /// </summary>
    /// <param name="initialCount">
    /// The initial number of concurrent entries permitted.
    /// </param>
    /// <param name="maxCount">
    /// The maximum number of concurrent entries permitted; also the most times
    /// <see cref="Release"/> may exceed the entries taken before it throws.
    /// </param>
    public FifoSemaphore(int initialCount, int maxCount)
    {
        semaphore = new SemaphoreSlim(initialCount, maxCount);
    }

    /// <summary>
    /// Releases all resources used by the underlying semaphore.
    /// </summary>
    public void Dispose() => semaphore.Dispose();

    /// <summary>
    /// Releases one slot, allowing the next queued waiter (in FIFO order) to enter.
    /// </summary>
    public void Release() => semaphore.Release();

    /// <summary>
    /// Asynchronously waits to enter the semaphore.
    /// </summary>
    /// <returns>
    /// A task that completes once entry is granted. Among all pending callers,
    /// entry is granted in the exact order this method was invoked.
    /// </returns>
    public async Task WaitAsync()
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        // Enqueue synchronously so the queue order matches the call order; this is
        // what makes entry FIFO regardless of which underlying wait completes first.
        queue.Enqueue(tcs);

        await semaphore.WaitAsync().ConfigureAwait(false);

        // Whichever wait completes hands its slot to the longest-waiting caller.
        if (queue.TryDequeue(out var popped))
        {
            popped.SetResult();
        }

        await tcs.Task.ConfigureAwait(false);
    }
}
