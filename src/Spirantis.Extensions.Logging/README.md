# Spirantis.Extensions.Logging

Serilog-based application logging for .NET. It wires Serilog into the generic host,
drives minimum levels from the `Logging` configuration section, and runs a set of
pluggable **logging sinks** (`ILoggingSink`) — the same composition pattern as
[`Spirantis.Extensions.Configuration`](https://www.nuget.org/packages/Spirantis.Extensions.Configuration).
It also provides structured **event logging** and structured **HTTP request/response**
helpers.

## Installation

```bash
dotnet add package Spirantis.Extensions.Logging
```

## Requirements

- .NET 10.0 or later

## Host integration

Both the classic `IHostBuilder` and the modern `IHostApplicationBuilder`
(`Host.CreateApplicationBuilder()`, `WebApplication.CreateBuilder()`) are supported:

```csharp
using Spirantis.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.UseLogging(options =>
{
    options.LoggingSinks.Add(new ConsoleSink());
    options.LoggingSinks.Add(new FileSink());
});
```

Minimum levels are read from configuration; absent a `Logging:LogLevel` section, sensible
defaults apply (`Information`, with `Microsoft`/`System` raised to `Warning`):

```json
{
  "Logging": {
    "LogLevel": { "Default": "Information", "Microsoft": "Warning" },
    "Console": { "Enabled": true }
  }
}
```

Each sink activates only when its own `Logging:<Name>:Enabled` flag is set, so sinks can
be registered unconditionally and toggled by configuration.

## Structured event logging

Model significant events as `LogEntryEvent<TKey>` subclasses and log them as structured
objects (captured under the `Event` property) rather than flattened into the message:

```csharp
public sealed class OrderPlaced(string orderId)
    : LogEntryEvent<string>(orderId, LogEntryEventType.Business, "Order placed");

logger.LogInformation(new OrderPlaced("order-123"));
```

## Structured REST logging

```csharp
logger.LogIncomingRequest("GET", "/orders", correlationKey: "abc");
logger.LogOutgoingResponse("POST", "/orders", statusCode: 201, elapsedMilliseconds: 12.5);
```

Each value (`Method`, `Path`, `StatusCode`, `ElapsedMilliseconds`, …) is captured as a named
structured property, so it stays queryable in your log store.

## Sink packages

- [`Spirantis.Extensions.Logging.Sink.Console`](https://www.nuget.org/packages/Spirantis.Extensions.Logging.Sink.Console) — themed text and structured JSON console output.
- [`Spirantis.Extensions.Logging.Sink.File`](https://www.nuget.org/packages/Spirantis.Extensions.Logging.Sink.File) — asynchronous rolling files.
- [`Spirantis.Extensions.Logging.Sink.AWSCloudWatch`](https://www.nuget.org/packages/Spirantis.Extensions.Logging.Sink.AWSCloudWatch) — AWS CloudWatch Logs.
- [`Spirantis.Extensions.Logging.Sink.Syslog`](https://www.nuget.org/packages/Spirantis.Extensions.Logging.Sink.Syslog) — local syslog (Linux).

## License

[MIT](LICENSE)
