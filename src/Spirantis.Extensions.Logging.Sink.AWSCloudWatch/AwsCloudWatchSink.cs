using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Formatting.Display;
using Serilog.Sinks.AwsCloudWatch;

namespace Spirantis.Extensions.Logging.Sink.AWSCloudWatch;

/// <summary>
/// A logging sink that writes to AWS CloudWatch Logs. Enabled by
/// <c>Logging:AWSCloudWatch:Enabled</c>; the log group, region, optional credentials, and
/// batching/retry behaviour are configured under the <c>Logging:AWSCloudWatch</c> section.
/// </summary>
public sealed class AwsCloudWatchSink : ILoggingSink
{
    /// <inheritdoc />
    public void Add(LoggerConfiguration loggerConfiguration, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(loggerConfiguration);

        var section = configuration.GetEnabledSinkSection("AWSCloudWatch");

        if (section is null)
        {
            return;
        }

        var options = new CloudWatchSinkOptions
        {
            LogGroupName = section.GetValue<string>("LogGroupName"),
            BatchSizeLimit = section.GetValue("BatchSizeLimit", 100),
            QueueSizeLimit = section.GetValue("QueueSizeLimit", 10000),
            Period = TimeSpan.FromSeconds(section.GetValue("PeriodSeconds", 10)),
            CreateLogGroup = true,
            LogStreamNameProvider = new DefaultLogStreamProvider(),
            RetryAttempts = section.GetValue<byte>("RetryAttempts", 5),
            TextFormatter = new MessageTemplateTextFormatter(
                LoggingSinkOptions.StandardOutputTemplate,
                formatProvider: null
            ),
        };

        loggerConfiguration.WriteTo.AmazonCloudWatch(options, CreateClient(section));
    }

    private static IAmazonCloudWatchLogs CreateClient(IConfiguration section)
    {
        var region = RegionEndpoint.GetBySystemName(section.GetValue<string>("Region"));

        string? accessKey = section.GetValue<string>("AccessKey");
        string? secretKey = section.GetValue<string>("SecretKey");

        return !string.IsNullOrWhiteSpace(accessKey) && !string.IsNullOrWhiteSpace(secretKey)
            ? new AmazonCloudWatchLogsClient(new BasicAWSCredentials(accessKey, secretKey), region)
            : new AmazonCloudWatchLogsClient(region);
    }
}
