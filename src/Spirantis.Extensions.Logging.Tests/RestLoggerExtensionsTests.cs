using Microsoft.Extensions.Logging;

namespace Spirantis.Extensions.Logging.Tests;

public sealed class RestLoggerExtensionsTests
{
    [Fact]
    public void LogIncomingRequest_CapturesNamedProperties()
    {
        var logger = new CapturingLogger();

        logger.LogIncomingRequest("GET", "/orders", correlationKey: "abc");

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Information, entry.Level);
        Assert.Equal("GET", entry.Property("Method"));
        Assert.Equal("/orders", entry.Property("Path"));
        Assert.Equal("abc", entry.Property("CorrelationKey"));
    }

    [Fact]
    public void LogOutgoingResponse_CapturesStatusTimingAndLevel()
    {
        var logger = new CapturingLogger();

        logger.LogOutgoingResponse(
            "POST",
            "/orders",
            statusCode: 201,
            elapsedMilliseconds: 12.5,
            retryCount: 2,
            level: LogLevel.Warning
        );

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Equal("POST", entry.Property("Method"));
        Assert.Equal(201, entry.Property("StatusCode"));
        Assert.Equal(12.5, entry.Property("ElapsedMilliseconds"));
        Assert.Equal(2, entry.Property("RetryCount"));
    }

    [Fact]
    public void LogIncomingResponse_AllowsNullStatusAndRetries()
    {
        var logger = new CapturingLogger();

        logger.LogIncomingResponse("GET", "/orders", statusCode: null, elapsedMilliseconds: 5);

        var entry = Assert.Single(logger.Entries);
        Assert.Null(entry.Property("StatusCode"));
        Assert.Null(entry.Property("RetryCount"));
    }
}
