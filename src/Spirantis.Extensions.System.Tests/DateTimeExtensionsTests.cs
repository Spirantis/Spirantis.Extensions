namespace Spirantis.Extensions.SystemTests;

/// <summary>Tests for the <see cref="DateTime" /> extensions declared in <c>DateTimeExtensions</c>.</summary>
[Trait("Category", "Unit")]
public sealed class DateTimeExtensionsTests
{
    [Theory]
    [InlineData(0, 0, 0, 0)]
    [InlineData(13, 30, 45, 810)]
    [InlineData(23, 59, 59, 1439)]
    public void ToMinutesSinceMidnight_ReturnsElapsedMinutes(
        int hour,
        int minute,
        int second,
        int expected
    )
    {
        DateTime dateTime = new(2024, 6, 8, hour, minute, second);
        Assert.Equal(expected, dateTime.ToMinutesSinceMidnight());
    }

    [Theory]
    [InlineData(0, 0, 0, 0)]
    [InlineData(13, 30, 45, 48645)]
    [InlineData(23, 59, 59, 86399)]
    public void ToSecondsSinceMidnight_ReturnsElapsedSeconds(
        int hour,
        int minute,
        int second,
        int expected
    )
    {
        DateTime dateTime = new(2024, 6, 8, hour, minute, second);
        Assert.Equal(expected, dateTime.ToSecondsSinceMidnight());
    }

    [Fact]
    public void ToMillisecondTimestamp_AtUnixEpoch_IsZero()
    {
        Assert.Equal(0d, DateTime.UnixEpoch.ToMillisecondTimestamp());
    }

    [Fact]
    public void ToMillisecondTimestamp_ReturnsMillisecondsSinceEpoch()
    {
        DateTime dateTime = DateTime.UnixEpoch.AddSeconds(1);
        Assert.Equal(1000d, dateTime.ToMillisecondTimestamp());
    }

    [Fact]
    public void ToMillisecondTimestamp_FloorsSubMillisecondValues()
    {
        DateTime dateTime = DateTime.UnixEpoch.AddTicks(15000); // 1.5 ms
        Assert.Equal(1d, dateTime.ToMillisecondTimestamp());
    }
}
