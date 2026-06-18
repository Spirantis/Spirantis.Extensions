using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;
using Spirantis.Extensions.Logging.Sink.Console.Enrichers;

namespace Spirantis.Extensions.Logging.Sink.Console;

/// <summary>
/// A logging sink that writes structured JSON to the console (one object per line),
/// omitting the redundant message text for structured events logged via
/// <see cref="EventLoggerExtensions"/>. Enabled by <c>Logging:Console:Enabled</c>.
/// </summary>
public sealed class JsonConsoleSink : ILoggingSink
{
    private const string JsonTemplate =
        "{ {Timestamp: @t, Level: @l, Message: if @mt = '"
        + EventLoggerExtensions.EventMessageTemplate
        + "' then undefined() else @mt, Exception: @x, ..@p} }\n";

    private static readonly ExpressionTemplate Template = new(
        JsonTemplate,
        theme: TemplateTheme.Code
    );

    /// <summary>
    /// Sets <see cref="Log.Logger"/> to a standalone JSON console logger — useful for tools
    /// and tests that run without a host.
    /// </summary>
    public static void SetDefaultLogger() =>
        Log.Logger = WriteToJsonConsole(new LoggerConfiguration().SetDefaultLogLevel())
            .CreateLogger();

    /// <inheritdoc />
    public void Add(LoggerConfiguration loggerConfiguration, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(loggerConfiguration);

        if (configuration.GetEnabledSinkSection("Console") is not null)
        {
            WriteToJsonConsole(loggerConfiguration);
        }
    }

    private static LoggerConfiguration WriteToJsonConsole(LoggerConfiguration configuration) =>
        configuration
            .Enrich.With(new RemoveTypeTagEnricher())
            .Enrich.FromLogContext()
            .WriteTo.Console(Template);
}
