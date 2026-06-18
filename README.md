# Spirantis.Extensions

A collection of lightweight extension libraries for .NET. Each library ships as its
own NuGet package so you only take what you need.

| Package | Description |
| ------- | ----------- |
| [`Spirantis.Extensions.System`](src/Spirantis.Extensions.System/README.md) | Extension methods for common `System` types — date/time conversions, span-aware string trimming, and reflection-based type discovery. |
| [`Spirantis.Extensions.Threading`](src/Spirantis.Extensions.Threading/README.md) | Asynchronous coordination primitives — FIFO-ordered async locking and serial execution of async work, globally or partitioned per key. |
| [`Spirantis.Extensions.Configuration`](src/Spirantis.Extensions.Configuration/README.md) | Composable application configuration — pluggable source agents layered over environment variables and command-line arguments. |
| [`Spirantis.Extensions.Configuration.Source.Json`](src/Spirantis.Extensions.Configuration.Source.Json/README.md) | Machine-wide and service-local JSON file configuration sources. |
| [`Spirantis.Extensions.Configuration.Source.AWSParameterStore`](src/Spirantis.Extensions.Configuration.Source.AWSParameterStore/README.md) | AWS Systems Manager Parameter Store configuration source with recursive loading and JSON flattening. |
| [`Spirantis.Extensions.Logging`](src/Spirantis.Extensions.Logging/README.md) | Serilog-based application logging — host integration, config-driven levels, a pluggable sink model, and structured event/REST helpers. |
| [`Spirantis.Extensions.Logging.Sink.Console`](src/Spirantis.Extensions.Logging.Sink.Console/README.md) | Themed text and structured JSON console logging sinks. |
| [`Spirantis.Extensions.Logging.Sink.File`](src/Spirantis.Extensions.Logging.Sink.File/README.md) | Asynchronous rolling-file logging sink. |
| [`Spirantis.Extensions.Logging.Sink.AWSCloudWatch`](src/Spirantis.Extensions.Logging.Sink.AWSCloudWatch/README.md) | AWS CloudWatch Logs sink. |
| [`Spirantis.Extensions.Logging.Sink.Syslog`](src/Spirantis.Extensions.Logging.Sink.Syslog/README.md) | Local syslog (RFC 5424) sink, Linux only. |

All packages target **.NET 10** and are **MIT** licensed. The `System` and `Threading`
packages are dependency-free; the `Configuration` packages build on
`Microsoft.Extensions.Configuration` (and the AWS SDK, for the Parameter Store source).

## Repository layout

```
Spirantis.Extensions.slnx              # solution tying everything together
src/
  .editorconfig                        # shared code style
  Directory.Build.props                # shared package metadata
  Spirantis.Extensions.System/         # library + its package README
  Spirantis.Extensions.System.Tests/
  Spirantis.Extensions.Threading/      # library + its package README
  Spirantis.Extensions.Threading.Tests/
  Spirantis.Extensions.Configuration/  # core configuration library
  Spirantis.Extensions.Configuration.Tests/
  Spirantis.Extensions.Configuration.Source.Json/             # JSON file sources
  Spirantis.Extensions.Configuration.Source.Json.Tests/
  Spirantis.Extensions.Configuration.Source.AWSParameterStore/ # AWS Parameter Store source
  Spirantis.Extensions.Configuration.Source.AWSParameterStore.Tests/
  Spirantis.Extensions.Logging/                # core Serilog logging library
  Spirantis.Extensions.Logging.Tests/
  Spirantis.Extensions.Logging.Sink.Console/   # console sinks (text + JSON)
  Spirantis.Extensions.Logging.Sink.Console.Tests/
  Spirantis.Extensions.Logging.Sink.File/      # rolling-file sink
  Spirantis.Extensions.Logging.Sink.AWSCloudWatch/ # AWS CloudWatch sink
  Spirantis.Extensions.Logging.Sink.Syslog/    # local syslog sink (Linux)
```

## Spirantis.Extensions.System

Extension methods for common `System` types. All extensions live in the `System`
namespace, so they're available without an extra `using` once the package is
referenced.

```bash
dotnet add package Spirantis.Extensions.System
```

### `DateTime` extensions

```csharp
DateTime now = DateTime.UtcNow;

// Milliseconds since the Unix epoch (floored)
double ms = now.ToMillisecondTimestamp();

// Minutes elapsed since midnight (0–1439)
int minutes = now.ToMinutesSinceMidnight();

// Seconds elapsed since midnight (0–86399)
int seconds = now.ToSecondsSinceMidnight();
```

### `string` / `ReadOnlySpan<char>` extensions

Trim a whole substring (not just individual characters) from either end, with a
configurable `StringComparison`:

```csharp
"file.txt.txt".TrimEnd(".txt", StringComparison.Ordinal);
// => "file"

"   prefix-prefix-value".AsSpan()
    .TrimStart("prefix-", StringComparison.Ordinal);
// => "value"  (span overload, allocation-free)
```

Overloads are provided for both `string` (returns `string`) and
`ReadOnlySpan<char>` (returns `ReadOnlySpan<char>`, no allocation).

### `IEnumerable<Type>` extensions

Find every type that derives from a given base type or open generic, at any depth:

```csharp
Type[] allTypes = typeof(MyClass).Assembly.GetTypes();

// Non-generic base type (transitive — all descendants)
IEnumerable<Type> shapes = allTypes.Derives(typeof(Shape));

// Open generic base type (e.g. all types deriving from Repository<T>)
IEnumerable<Type> repos = allTypes.Derives(typeof(Repository<>));
```

## Spirantis.Extensions.Threading

Asynchronous coordination primitives that fill gaps in the built-in BCL types.

```bash
dotnet add package Spirantis.Extensions.Threading
```

- **`FifoSemaphore`** — an async semaphore that grants entry in strict first-in,
  first-out order (`SemaphoreSlim` does not guarantee ordering).
- **`FunctionExecutionQueue`** — runs async actions one at a time, in enqueue order.
- **`KeyedFifoSemaphore`** — per-key FIFO mutual exclusion; locks on the same key
  serialize, different keys run concurrently.
- **`KeyedFunctionExecutionQueue<TKey>`** — a `FunctionExecutionQueue` per key.

See the [package README](src/Spirantis.Extensions.Threading/README.md) for full
usage examples.

## Spirantis.Extensions.Configuration

Composable application configuration. `AddConfiguration` layers environment variables
and command-line arguments, runs a set of pluggable `IConfigurationSourceAgent`
instances, then re-applies command-line arguments so they take final precedence.

```bash
dotnet add package Spirantis.Extensions.Configuration
```

```csharp
using Microsoft.Extensions.Configuration;
using Spirantis.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddConfiguration(
        switchMappings: new Dictionary<string, string>(),
        commandLineArgs: args,
        options =>
        {
            options.ConfigurationSourceAgents.Add(new ServiceJsonFileSourceAgent());
            options.ConfigurationSourceAgents.Add(new AWSParameterStoreSourceAgent());
        })
    .Build();
```

Two source agent packages are provided:

- **`Spirantis.Extensions.Configuration.Source.Json`** — `ServiceJsonFileSourceAgent`
  (service-local `config.json`) and `GlobalJsonFileSourceAgent` (machine-wide
  `config.json` discovered across configurable, origin-neutral search paths).
- **`Spirantis.Extensions.Configuration.Source.AWSParameterStore`** —
  `AWSParameterStoreSourceAgent`, loading parameters recursively from AWS Systems
  Manager Parameter Store and flattening JSON values into configuration keys.

See the package READMEs
([core](src/Spirantis.Extensions.Configuration/README.md),
[JSON](src/Spirantis.Extensions.Configuration.Source.Json/README.md),
[AWS](src/Spirantis.Extensions.Configuration.Source.AWSParameterStore/README.md))
for full usage details.

## Spirantis.Extensions.Logging

Serilog-based application logging. `UseLogging` wires Serilog into the host (classic
`IHostBuilder` or modern `IHostApplicationBuilder`), drives minimum levels from the
`Logging` configuration section, and runs a set of pluggable `ILoggingSink` outputs —
the same composition pattern as the configuration package.

```bash
dotnet add package Spirantis.Extensions.Logging
```

```csharp
using Spirantis.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.UseLogging(options =>
{
    options.LoggingSinks.Add(new ConsoleSink());
    options.LoggingSinks.Add(new FileSink());
});
```

Each sink is its own package and activates only when its `Logging:<Name>:Enabled` flag is
set: **Console** (themed text + structured JSON), **File** (async rolling), **AWSCloudWatch**,
and **Syslog** (Linux). The core also provides structured **event logging**
(`LogEntryEvent<TKey>` + `logger.LogInformation(event)`) and structured **REST** helpers
(`logger.LogIncomingRequest(...)`), both capturing named properties rather than flattening
them into the message text.

See the [package README](src/Spirantis.Extensions.Logging/README.md) for full usage details.

## Building & testing

Build and test the whole solution:

```bash
dotnet build Spirantis.Extensions.slnx
dotnet test  Spirantis.Extensions.slnx
```

Code is formatted with [CSharpier](https://csharpier.com):

```bash
csharpier format src/
```

## Publishing

Each library is packed and published to [nuget.org](https://www.nuget.org) as a
separate package. Shared metadata (version, authors, license, symbol packages)
lives in `src/Directory.Build.props`; per-package metadata (id, description, tags)
lives in each project file.

```bash
dotnet pack src/Spirantis.Extensions.System/Spirantis.Extensions.System.csproj -c Release
dotnet pack src/Spirantis.Extensions.Threading/Spirantis.Extensions.Threading.csproj -c Release
dotnet pack src/Spirantis.Extensions.Configuration/Spirantis.Extensions.Configuration.csproj -c Release
dotnet pack src/Spirantis.Extensions.Configuration.Source.Json/Spirantis.Extensions.Configuration.Source.Json.csproj -c Release
dotnet pack src/Spirantis.Extensions.Configuration.Source.AWSParameterStore/Spirantis.Extensions.Configuration.Source.AWSParameterStore.csproj -c Release
dotnet pack src/Spirantis.Extensions.Logging/Spirantis.Extensions.Logging.csproj -c Release
dotnet pack src/Spirantis.Extensions.Logging.Sink.Console/Spirantis.Extensions.Logging.Sink.Console.csproj -c Release
dotnet pack src/Spirantis.Extensions.Logging.Sink.File/Spirantis.Extensions.Logging.Sink.File.csproj -c Release
dotnet pack src/Spirantis.Extensions.Logging.Sink.AWSCloudWatch/Spirantis.Extensions.Logging.Sink.AWSCloudWatch.csproj -c Release
dotnet pack src/Spirantis.Extensions.Logging.Sink.Syslog/Spirantis.Extensions.Logging.Sink.Syslog.csproj -c Release
```

## License

[MIT](LICENSE)
