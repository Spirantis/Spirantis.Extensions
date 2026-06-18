using static Spirantis.Extensions.Threading.Tests.ConcurrencyTestHelpers;

namespace Spirantis.Extensions.Threading.Tests;

public sealed class KeyedFifoSemaphoreTests
{
    [Fact]
    public async Task SameKey_IsMutuallyExclusive()
    {
        var keyed = new KeyedFifoSemaphore();

        var first = await keyed.WaitAsync("k");

        var secondTask = keyed.WaitAsync("k");
        await Task.Delay(100);
        Assert.False(secondTask.IsCompleted); // blocked behind the first holder

        first.Dispose();

        var second = await secondTask.WaitAsync(WaitTimeout);
        second.Dispose();
    }

    [Fact]
    public async Task DifferentKeys_AcquireConcurrently()
    {
        var keyed = new KeyedFifoSemaphore();

        var a = await keyed.WaitAsync("a");
        // Different key must not block even while "a" is held.
        var b = await keyed.WaitAsync("b").WaitAsync(WaitTimeout);

        a.Dispose();
        b.Dispose();
        Assert.True(keyed.IsEmpty);
    }

    [Fact]
    public async Task LastReleaser_RemovesKey_AndFiresEvent()
    {
        var keyed = new KeyedFifoSemaphore();
        string? firedKey = null;
        keyed.LastProcessForKey += (_, key) => firedKey = key;

        var releaser = await keyed.WaitAsync("k");
        Assert.True(keyed.ContainsKey("k"));

        releaser.Dispose();

        Assert.Equal("k", firedKey);
        Assert.False(keyed.ContainsKey("k"));
        Assert.True(keyed.IsEmpty);
    }

    [Fact]
    public async Task Releaser_DoubleDispose_IsSafe()
    {
        var keyed = new KeyedFifoSemaphore();

        var releaser = await keyed.WaitAsync("k");
        releaser.Dispose();
        releaser.Dispose(); // must not throw or double-decrement

        Assert.True(keyed.IsEmpty);

        // The key must be re-acquirable afterwards.
        var again = await keyed.WaitAsync("k").WaitAsync(WaitTimeout);
        again.Dispose();
    }

    [Fact]
    public async Task ConcurrentWaiters_SameKey_StayMutuallyExclusive_AndDrain()
    {
        var keyed = new KeyedFifoSemaphore();
        const int Waiters = 200;

        int concurrent = 0;
        int maxConcurrent = 0;
        int count = 0;

        var tasks = Enumerable
            .Range(0, Waiters)
            .Select(async _ =>
            {
                using var releaser = await keyed.WaitAsync("k");
                int current = Interlocked.Increment(ref concurrent);
                InterlockedMax(ref maxConcurrent, current);
                await Task.Yield();
                Interlocked.Increment(ref count);
                Interlocked.Decrement(ref concurrent);
            });

        await Task.WhenAll(tasks).WaitAsync(WaitTimeout);

        Assert.Equal(1, Volatile.Read(ref maxConcurrent)); // only one holder at a time
        Assert.Equal(Waiters, Volatile.Read(ref count));
        Assert.True(keyed.IsEmpty); // ref-counted entry fully drained
    }
}
