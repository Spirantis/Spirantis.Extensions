# Spirantis.Extensions.Logging.Sink.File

A rolling-file logging sink for
[`Spirantis.Extensions.Logging`](https://www.nuget.org/packages/Spirantis.Extensions.Logging),
writing asynchronously via Serilog.

## Installation

```bash
dotnet add package Spirantis.Extensions.Logging.Sink.File
```

## Usage

```csharp
builder.UseLogging(options => options.LoggingSinks.Add(new FileSink()));
```

Activates when `Logging:File:Enabled` is `true`. Configurable under `Logging:File`:

| Key | Default | Purpose |
| --- | ------- | ------- |
| `Path` | `<base>/Logs` | Target directory; `{ServiceName}` is replaced with the entry assembly name. |
| `Encoding` | `UTF-8` | File encoding. |
| `FileSizeLimit` | `10000000` | Bytes per file before rolling. |
| `RetainedFileCountLimit` | `31` | Number of rolled files to keep. |

Files roll daily and on size limit, and are written through an async buffer.

## License

[MIT](LICENSE)
