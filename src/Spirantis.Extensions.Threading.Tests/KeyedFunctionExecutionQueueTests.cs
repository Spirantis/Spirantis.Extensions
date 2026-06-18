using static Spirantis.Extensions.Threading.Tests.ConcurrencyTestHelpers;

namespace Spirantis.Extensions.Threading.Tests;

public sealed class KeyedFunctionExecutionQueueTests
{
    [Fact]
    public async Task SameKey_SecondAction_WaitsForFirst()
    {
        var queue = new KeyedFunctionExecutionQueue<string>();
        var firstStarted = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var firstCanFinish = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var secondStarted = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        queue.Enqueue(
            "k",
            async () =>
            {
                firstStarted.TrySetResult();
                await firstCanFinish.Task;
            }
        );
        queue.Enqueue(
            "k",
            () =>
            {
                secondStarted.TrySetResult();
                return Task.CompletedTask;
            }
        );

        await firstStarted.Task.WaitAsync(WaitTimeout);

        await Task.Delay(100);
        Assert.False(secondStarted.Task.IsCompleted); // blocked behind the first

        firstCanFinish.TrySetResult();
        await secondStarted.Task.WaitAsync(WaitTimeout);
    }

    [Fact]
    public async Task DifferentKeys_RunConcurrently()
    {
        var queue = new KeyedFunctionExecutionQueue<string>();
        var aStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var bStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        queue.Enqueue(
            "a",
            async () =>
            {
                aStarted.TrySetResult();
                await release.Task;
            }
        );
        queue.Enqueue(
            "b",
            async () =>
            {
                bStarted.TrySetResult();
                await release.Task;
            }
        );

        // If different keys did not run concurrently, one would block the other
        // and this would time out.
        await Task.WhenAll(aStarted.Task, bStarted.Task).WaitAsync(WaitTimeout);

        release.TrySetResult();
    }

    [Fact]
    public async Task DrainingKey_FiresOnLastAction_AndRemovesEntry()
    {
        var queue = new KeyedFunctionExecutionQueue<string>();
        var last = new TaskCompletionSource<string>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        queue.OnLastAction += key => last.TrySetResult(key);

        queue.Enqueue("x", async () => await Task.Yield());

        string firedKey = await last.Task.WaitAsync(WaitTimeout);
        Assert.Equal("x", firedKey);
        Assert.True(queue.IsEmpty);
        Assert.False(queue.Contains("x"));
    }

    [Fact]
    public async Task FailingAction_DoesNotWedgeKey_OrOtherKeys()
    {
        var queue = new KeyedFunctionExecutionQueue<string>();

        var ranAfterFailureSameKey = false;
        var ranOtherKey = false;

        int executed = 0;
        var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        queue.OnActionExecuted += _ =>
        {
            if (Interlocked.Increment(ref executed) == 3)
            {
                done.TrySetResult();
            }
        };

        queue.Enqueue("k", () => throw new InvalidOperationException("boom"));
        queue.Enqueue(
            "k",
            async () =>
            {
                await Task.Yield();
                ranAfterFailureSameKey = true;
            }
        );
        queue.Enqueue(
            "other",
            async () =>
            {
                await Task.Yield();
                ranOtherKey = true;
            }
        );

        await done.Task.WaitAsync(WaitTimeout);

        Assert.True(ranAfterFailureSameKey); // same key keeps draining after a failure
        Assert.True(ranOtherKey); // other keys are unaffected
        Assert.True(queue.IsEmpty); // everything drained and was removed
    }

    [Fact]
    public async Task ConcurrentEnqueue_SameKeys_StaySerialized_AndDrain()
    {
        var queue = new KeyedFunctionExecutionQueue<int>();
        const int Keys = 4;
        const int PerKey = 100;
        const int Total = Keys * PerKey;

        int[] concurrent = new int[Keys];
        int[] maxConcurrent = new int[Keys];
        int[] counts = new int[Keys];

        int executed = 0;
        var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        queue.OnActionExecuted += _ =>
        {
            if (Interlocked.Increment(ref executed) == Total)
            {
                done.TrySetResult();
            }
        };

        // Fan out enqueues across threads to provoke the add/remove race.
        Parallel.For(
            0,
            Total,
            i =>
            {
                int key = i % Keys;
                queue.Enqueue(
                    key,
                    async () =>
                    {
                        int current = Interlocked.Increment(ref concurrent[key]);
                        InterlockedMax(ref maxConcurrent[key], current);
                        await Task.Yield();
                        Interlocked.Increment(ref counts[key]);
                        Interlocked.Decrement(ref concurrent[key]);
                    }
                );
            }
        );

        await done.Task.WaitAsync(WaitTimeout);

        for (int key = 0; key < Keys; key++)
        {
            Assert.Equal(1, Volatile.Read(ref maxConcurrent[key])); // never overlapped per key
            Assert.Equal(PerKey, Volatile.Read(ref counts[key])); // every action ran exactly once
        }

        Assert.True(queue.IsEmpty); // every key drained and was removed
    }
}
