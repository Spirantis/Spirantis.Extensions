namespace System;

public static class DateTimeExtensions
{
    extension(DateTime dateTime)
    {
        public double ToMillisecondTimestamp()
        {
            var timeSpan = dateTime - DateTime.UnixEpoch;
            return Math.Floor(timeSpan.TotalMilliseconds);
        }

        public int ToMinutesSinceMidnight() => (dateTime.Hour * 60) + dateTime.Minute;

        public int ToSecondsSinceMidnight() =>
            (dateTime.ToMinutesSinceMidnight() * 60) + dateTime.Second;
    }
}
