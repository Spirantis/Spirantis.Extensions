# Spirantis.Extensions.Logging.Sink.Console

Console logging sinks for
[`Spirantis.Extensions.Logging`](https://www.nuget.org/packages/Spirantis.Extensions.Logging):
a themed, human-readable text sink and a structured JSON sink.

## Installation

```bash
dotnet add package Spirantis.Extensions.Logging.Sink.Console
```

## Usage

Register either sink; both activate when `Logging:Console:Enabled` is `true`.

```csharp
builder.UseLogging(options => options.LoggingSinks.Add(new ConsoleSink()));
// or, for one structured-JSON object per line:
builder.UseLogging(options => options.LoggingSinks.Add(new JsonConsoleSink()));
```

`JsonConsoleSink` emits structured JSON (via Serilog's `ExpressionTemplate`), strips the
synthetic `$type` tag from structured objects (`RemoveTypeTagEnricher`), and omits the
redundant message text for events logged with `LogEntryEvent`.

Both sinks also expose a static `SetDefaultLogger()` for tools and tests that run without a
host.

## License

[MIT](LICENSE)
