namespace System;

/// <summary>Extension methods for <see cref="DateTime"/>.</summary>
public static class DateTimeExtensions
{
    extension(DateTime dateTime)
    {
        /// <summary>
        /// Gets the number of whole milliseconds elapsed since the Unix epoch
        /// (1970-01-01T00:00:00Z), floored.
        /// </summary>
        /// <returns>The millisecond timestamp.</returns>
        public double ToMillisecondTimestamp()
        {
            var timeSpan = dateTime - DateTime.UnixEpoch;
            return Math.Floor(timeSpan.TotalMilliseconds);
        }

        /// <summary>Gets the number of whole minutes elapsed since midnight (0–1439).</summary>
        /// <returns>The minutes since midnight.</returns>
        public int ToMinutesSinceMidnight() => (dateTime.Hour * 60) + dateTime.Minute;

        /// <summary>Gets the number of whole seconds elapsed since midnight (0–86399).</summary>
        /// <returns>The seconds since midnight.</returns>
        public int ToSecondsSinceMidnight() =>
            (dateTime.ToMinutesSinceMidnight() * 60) + dateTime.Second;
    }
}
