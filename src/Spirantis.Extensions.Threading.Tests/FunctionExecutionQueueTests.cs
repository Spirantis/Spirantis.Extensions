using static Spirantis.Extensions.Threading.Tests.ConcurrencyTestHelpers;

namespace Spirantis.Extensions.Threading.Tests;

public sealed class FunctionExecutionQueueTests
{
    [Fact]
    public async Task Enqueue_RunsActions_Sequentially_InOrder()
    {
        var queue = new FunctionExecutionQueue();
        var results = new List<int>();
        const int Count = 50;

        int executed = 0;
        var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        queue.OnActionExecuted += () =>
        {
            if (Interlocked.Increment(ref executed) == Count)
            {
                done.TrySetResult();
            }
        };

        for (int i = 0; i < Count; i++)
        {
            int captured = i;
            queue.Enqueue(async () =>
            {
                await Task.Yield();
                results.Add(captured); // safe without a lock only because execution is serial
            });
        }

        await done.Task.WaitAsync(WaitTimeout);
        Assert.Equal(Enumerable.Range(0, Count), results);
    }

    [Fact]
    public async Task Enqueue_NeverRunsTwoActionsConcurrently()
    {
        var queue = new FunctionExecutionQueue();
        const int Count = 25;

        int concurrent = 0;
        int maxConcurrent = 0;
        int executed = 0;
        var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        queue.OnActionExecuted += () =>
        {
            if (Interlocked.Increment(ref executed) == Count)
            {
                done.TrySetResult();
            }
        };

        for (int i = 0; i < Count; i++)
        {
            queue.Enqueue(async () =>
            {
                int current = Interlocked.Increment(ref concurrent);
                InterlockedMax(ref maxConcurrent, current);
                await Task.Delay(5);
                Interlocked.Decrement(ref concurrent);
            });
        }

        await done.Task.WaitAsync(WaitTimeout);
        Assert.Equal(1, Volatile.Read(ref maxConcurrent));
    }

    [Fact]
    public async Task OnLastAction_Fires_WhenQueueDrains()
    {
        var queue = new FunctionExecutionQueue();
        var lastFired = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        queue.OnLastAction += () => lastFired.TrySetResult();

        queue.Enqueue(async () => await Task.Yield());

        await lastFired.Task.WaitAsync(WaitTimeout);
    }

    [Fact]
    public async Task FailingAction_DoesNotBreakTheQueue()
    {
        var queue = new FunctionExecutionQueue();
        var ranAfterFailure = false;

        int executed = 0;
        var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        queue.OnActionExecuted += () =>
        {
            if (Interlocked.Increment(ref executed) == 2)
            {
                done.TrySetResult();
            }
        };

        var lastFired = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        queue.OnLastAction += () => lastFired.TrySetResult();

        queue.Enqueue(() => throw new InvalidOperationException("boom"));
        queue.Enqueue(async () =>
        {
            await Task.Yield();
            ranAfterFailure = true;
        });

        await done.Task.WaitAsync(WaitTimeout);
        await lastFired.Task.WaitAsync(WaitTimeout);

        Assert.True(ranAfterFailure); // a failing action must not stall the following one
        Assert.Equal(2, Volatile.Read(ref executed)); // both actions counted (counter not desynced)
    }

    [Fact]
    public async Task OnActionExecuted_Fires_OncePerAction()
    {
        var queue = new FunctionExecutionQueue();
        const int Count = 30;

        int executed = 0;
        var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        queue.OnActionExecuted += () =>
        {
            if (Interlocked.Increment(ref executed) == Count)
            {
                done.TrySetResult();
            }
        };

        for (int i = 0; i < Count; i++)
        {
            queue.Enqueue(async () => await Task.Yield());
        }

        await done.Task.WaitAsync(WaitTimeout);
        // Give any erroneous extra invocations a chance to land before asserting.
        await Task.Delay(100);
        Assert.Equal(Count, Volatile.Read(ref executed));
    }
}
