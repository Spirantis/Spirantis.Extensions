using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Spirantis.Extensions.Logging.Sink.Console;

/// <summary>
/// A logging sink that writes themed, human-readable text to the console.
/// Enabled by the <c>Logging:Console:Enabled</c> configuration value.
/// </summary>
public sealed class ConsoleSink : ILoggingSink
{
    /// <summary>
    /// Sets <see cref="Log.Logger"/> to a standalone console logger — useful for tools and
    /// tests that run without a host.
    /// </summary>
    public static void SetDefaultLogger() =>
        Log.Logger = WriteToConsole(new LoggerConfiguration().SetDefaultLogLevel()).CreateLogger();

    /// <inheritdoc />
    public void Add(LoggerConfiguration loggerConfiguration, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(loggerConfiguration);

        if (configuration.GetEnabledSinkSection("Console") is not null)
        {
            WriteToConsole(loggerConfiguration);
        }
    }

    private static LoggerConfiguration WriteToConsole(LoggerConfiguration configuration) =>
        configuration.WriteTo.Console(
            outputTemplate: LoggingSinkOptions.StandardOutputTemplate,
            theme: AnsiConsoleTheme.Code
        );
}
