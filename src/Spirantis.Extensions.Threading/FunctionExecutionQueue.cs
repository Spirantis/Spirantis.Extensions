namespace Spirantis.Extensions.Threading;

/// <summary>
/// Executes asynchronous actions one at a time, in the order they are enqueued.
/// </summary>
/// <remarks>
/// Each enqueued action starts only after the previous one has fully completed,
/// so actions never overlap. <see cref="Enqueue"/> returns immediately; actions
/// run on the thread pool.
/// </remarks>
public sealed class FunctionExecutionQueue
{
    private readonly Lock locker = new();

    private Task lastTask = Task.CompletedTask;

    private int referenceCount;

    /// <summary>
    /// Raised when the queue drains, i.e. the action that just completed left no
    /// further actions pending.
    /// </summary>
    public event Action? OnLastAction;

    /// <summary>
    /// Raised after each enqueued action completes, whether it succeeded or threw.
    /// </summary>
    public event Action? OnActionExecuted;

    /// <summary>
    /// Enqueues an action to run after all previously enqueued actions complete.
    /// </summary>
    /// <param name="asyncAction">The asynchronous action to execute.</param>
    public void Enqueue(Func<Task> asyncAction)
    {
        using (locker.EnterScope())
        {
            ++referenceCount;
            lastTask = RunSerially(lastTask, asyncAction);
        }
    }

    /// <summary>
    /// Awaits the previous action, then runs <paramref name="asyncAction"/> and
    /// decrements the pending counter, forming the serial chain.
    /// </summary>
    private async Task RunSerially(Task previous, Func<Task> asyncAction)
    {
        await previous.ConfigureAwait(false);

        try
        {
            // Task.Run keeps the action off the caller's thread and out from under the
            // lock, matching the original cold-path behaviour.
            await Task.Run(asyncAction).ConfigureAwait(false);
        }
        catch
        {
            // Swallow: a failing action must not break the chain or desync the counter.
        }
        finally
        {
            SubtractCounter();
        }
    }

    /// <summary>
    /// Decrements the pending-action counter and raises the completion events,
    /// firing <see cref="OnLastAction"/> when the queue has drained.
    /// </summary>
    private void SubtractCounter()
    {
        bool isLast;

        using (locker.EnterScope())
        {
            isLast = --referenceCount == 0;
        }

        OnActionExecuted?.Invoke();

        if (isLast)
        {
            OnLastAction?.Invoke();
        }
    }
}
