using static Spirantis.Extensions.Threading.Tests.ConcurrencyTestHelpers;

namespace Spirantis.Extensions.Threading.Tests;

public sealed class FifoSemaphoreTests
{
    [Fact]
    public async Task WaitAsync_CompletesImmediately_WhenSlotAvailable()
    {
        using var semaphore = new FifoSemaphore(1);

        var wait = semaphore.WaitAsync();

        await wait.WaitAsync(WaitTimeout);
        Assert.True(wait.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task WaitAsync_Blocks_UntilRelease()
    {
        using var semaphore = new FifoSemaphore(1);

        await semaphore.WaitAsync(); // take the only slot

        var blocked = semaphore.WaitAsync();
        await Task.Delay(100);
        Assert.False(blocked.IsCompleted);

        semaphore.Release();

        await blocked.WaitAsync(WaitTimeout);
        Assert.True(blocked.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task WaitAsync_ReleasesWaiters_InFifoOrder()
    {
        using var semaphore = new FifoSemaphore(1);

        await semaphore.WaitAsync(); // take the only slot

        // Enqueued synchronously, so the queue order is deterministic.
        var first = semaphore.WaitAsync();
        var second = semaphore.WaitAsync();
        var third = semaphore.WaitAsync();

        // One release frees exactly one slot, which must go to the head of the queue.
        semaphore.Release();
        await first.WaitAsync(WaitTimeout);
        Assert.False(second.IsCompleted);
        Assert.False(third.IsCompleted);

        semaphore.Release();
        await second.WaitAsync(WaitTimeout);
        Assert.False(third.IsCompleted);

        semaphore.Release();
        await third.WaitAsync(WaitTimeout);
    }

    [Fact]
    public async Task WaitAsync_AllowsConfiguredConcurrency()
    {
        using var semaphore = new FifoSemaphore(2);

        var first = semaphore.WaitAsync();
        var second = semaphore.WaitAsync();
        var third = semaphore.WaitAsync();

        await Task.WhenAll(first, second).WaitAsync(WaitTimeout);
        Assert.False(third.IsCompleted); // third must wait for a release

        semaphore.Release();
        await third.WaitAsync(WaitTimeout);
    }
}
