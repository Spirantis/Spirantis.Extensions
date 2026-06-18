# Spirantis.Extensions.Logging.Sink.Syslog

A local syslog (RFC 5424) logging sink for
[`Spirantis.Extensions.Logging`](https://www.nuget.org/packages/Spirantis.Extensions.Logging).
**Linux only** — it's a no-op on other platforms.

## Installation

```bash
dotnet add package Spirantis.Extensions.Logging.Sink.Syslog
```

## Usage

```csharp
builder.UseLogging(options => options.LoggingSinks.Add(new SyslogSink()));
```

Activates when `Logging:Syslog:Enabled` is `true` and the process runs on Linux. The syslog
facility is set via `Logging:Syslog:FacilityCode` (an integer; defaults to `Local5`).

The package also exposes `LoggerSinkConfiguration.LocalRfc5424Syslog(...)` for direct Serilog
configuration.

## License

[MIT](LICENSE)
