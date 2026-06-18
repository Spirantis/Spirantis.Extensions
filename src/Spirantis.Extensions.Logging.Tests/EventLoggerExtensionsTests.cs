using Microsoft.Extensions.Logging;

namespace Spirantis.Extensions.Logging.Tests;

public sealed class EventLoggerExtensionsTests
{
    private sealed class SampleEvent(string key) : LogEntryEvent<string>(key);

    [Fact]
    public void LogInformation_LogsEventAsStructuredProperty()
    {
        var logger = new CapturingLogger();
        var sampleEvent = new SampleEvent("order-1");

        logger.LogInformation(sampleEvent);

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Information, entry.Level);
        Assert.Contains(entry.Properties, pair => ReferenceEquals(pair.Value, sampleEvent));
    }

    [Fact]
    public void LogWarning_LogsAtWarningLevel()
    {
        var logger = new CapturingLogger();

        logger.LogWarning(new SampleEvent("k"));

        Assert.Equal(LogLevel.Warning, Assert.Single(logger.Entries).Level);
    }

    [Fact]
    public void LogError_WithException_CapturesExceptionAndEvent()
    {
        var logger = new CapturingLogger();
        var exception = new InvalidOperationException("boom");
        var sampleEvent = new SampleEvent("k");

        logger.LogError(exception, sampleEvent);

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Same(exception, entry.Exception);
        Assert.Contains(entry.Properties, pair => ReferenceEquals(pair.Value, sampleEvent));
    }
}
