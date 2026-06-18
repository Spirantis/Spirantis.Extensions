# Spirantis.Extensions.Logging.Sink.AWSCloudWatch

An AWS CloudWatch Logs sink for
[`Spirantis.Extensions.Logging`](https://www.nuget.org/packages/Spirantis.Extensions.Logging).

## Installation

```bash
dotnet add package Spirantis.Extensions.Logging.Sink.AWSCloudWatch
```

## Usage

```csharp
builder.UseLogging(options => options.LoggingSinks.Add(new AwsCloudWatchSink()));
```

Activates when `Logging:AWSCloudWatch:Enabled` is `true`. Configurable under
`Logging:AWSCloudWatch`:

| Key | Default | Purpose |
| --- | ------- | ------- |
| `LogGroupName` | — | The CloudWatch log group (created if missing). |
| `Region` | — | AWS region system name (e.g. `eu-west-1`). |
| `AccessKey` / `SecretKey` | — | Optional explicit credentials; omitted to use the default AWS credential chain. |
| `BatchSizeLimit` | `100` | Events per batch. |
| `QueueSizeLimit` | `10000` | Max queued events. |
| `PeriodSeconds` | `10` | Flush interval. |
| `RetryAttempts` | `5` | Delivery retry attempts. |

Uses AWS SDK v4 (`AWSSDK.CloudWatchLogs`).

## License

[MIT](LICENSE)
