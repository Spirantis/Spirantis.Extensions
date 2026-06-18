namespace Spirantis.Extensions.Threading;

/// <summary>
/// Maintains a separate <see cref="FunctionExecutionQueue"/> per key, so that
/// actions sharing a key run serially while actions with different keys run
/// concurrently.
/// </summary>
/// <remarks>
/// A key's queue is created on first use and removed once it drains, so a key
/// occupies no memory while idle.
/// </remarks>
/// <typeparam name="TKey">The type used to partition actions into queues.</typeparam>
public sealed class KeyedFunctionExecutionQueue<TKey>
    where TKey : notnull
{
    private readonly Dictionary<TKey, ReferenceCounter<FunctionExecutionQueue>> queues = [];
    private readonly Lock locker = new();

    /// <summary>
    /// Raised with the key when that key's queue drains and is removed.
    /// </summary>
    public event Action<TKey>? OnLastAction;

    /// <summary>
    /// Raised with the key after each action for that key completes.
    /// </summary>
    public event Action<TKey>? OnActionExecuted;

    /// <summary>
    /// Gets a value indicating whether no keys currently have pending actions.
    /// </summary>
    public bool IsEmpty
    {
        get
        {
            using (locker.EnterScope())
            {
                return queues.Count == 0;
            }
        }
    }

    /// <summary>
    /// Returns whether the given key currently has pending actions.
    /// </summary>
    /// <param name="key">The key to test.</param>
    public bool Contains(TKey key)
    {
        using (locker.EnterScope())
        {
            return queues.ContainsKey(key);
        }
    }

    /// <summary>
    /// Enqueues an action under the given key. It runs after all previously
    /// enqueued actions for the same key, and may run concurrently with actions
    /// under other keys.
    /// </summary>
    /// <param name="key">The key whose queue the action is appended to.</param>
    /// <param name="function">The asynchronous action to execute.</param>
    public void Enqueue(TKey key, Func<Task> function)
    {
        using (locker.EnterScope())
        {
            if (queues.TryGetValue(key, out var counter))
            {
                ++counter.Count;
            }
            else
            {
                var queue = new FunctionExecutionQueue();
                queue.OnActionExecuted += () => OnActionCompleted(key);
                counter = new ReferenceCounter<FunctionExecutionQueue>(queue);
                queues[key] = counter;
            }

            counter.Value.Enqueue(function);
        }
    }

    /// <summary>
    /// Decrements the key's pending count and, once it reaches zero, removes the
    /// key and raises <see cref="OnLastAction"/>. The increment in
    /// <see cref="Enqueue"/> and this decrement share <c>locker</c>, so a key is
    /// only removed while genuinely idle.
    /// </summary>
    private void OnActionCompleted(TKey key)
    {
        bool isLast = false;

        using (locker.EnterScope())
        {
            var counter = queues[key];

            if (--counter.Count == 0)
            {
                queues.Remove(key);
                isLast = true;
            }
        }

        OnActionExecuted?.Invoke(key);

        if (isLast)
        {
            OnLastAction?.Invoke(key);
        }
    }
}
