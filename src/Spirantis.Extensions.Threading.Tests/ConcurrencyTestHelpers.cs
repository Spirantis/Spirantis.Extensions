namespace Spirantis.Extensions.Threading.Tests;

internal static class ConcurrencyTestHelpers
{
    /// <summary>
    /// Default timeout for awaiting test signals. Generous enough to avoid
    /// flakiness on a loaded CI machine, short enough to fail fast on a hang.
    /// </summary>
    public static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Atomically raises <paramref name="target"/> to <paramref name="value"/>
    /// if the new value is larger. Used to record peak observed concurrency.
    /// </summary>
    public static void InterlockedMax(ref int target, int value)
    {
        int current;

        do
        {
            current = Volatile.Read(ref target);

            if (value <= current)
            {
                return;
            }
        } while (Interlocked.CompareExchange(ref target, value, current) != current);
    }
}
