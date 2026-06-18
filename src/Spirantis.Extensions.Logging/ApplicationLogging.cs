using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace Spirantis.Extensions.Logging;

/// <summary>
/// Configures Serilog-based application logging: host integration, configuration-driven
/// minimum levels, and a pluggable set of <see cref="ILoggingSink"/> outputs.
/// </summary>
public static class ApplicationLogging
{
    private static readonly SerilogLoggerProvider SerilogLoggerProvider = new();

    /// <summary>Creates an <see cref="Microsoft.Extensions.Logging.ILogger"/> for a category.</summary>
    public static Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName) =>
        SerilogLoggerProvider.CreateLogger(categoryName);

    /// <summary>
    /// Applies sensible default minimum levels: <see cref="LogEventLevel.Information"/> overall,
    /// with the <c>Microsoft</c> and <c>System</c> namespaces raised to <see cref="LogEventLevel.Warning"/>.
    /// </summary>
    public static LoggerConfiguration SetDefaultLogLevel(this LoggerConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning);
    }

    /// <summary>
    /// Configures Serilog on a classic <see cref="IHostBuilder"/>, driving levels from the
    /// <c>Logging</c> configuration section and invoking the configured sinks.
    /// </summary>
    public static IHostBuilder UseLogging(
        this IHostBuilder hostBuilder,
        Action<LoggingSinkOptions>? configureSinks = null
    )
    {
        ArgumentNullException.ThrowIfNull(hostBuilder);

        return hostBuilder.UseSerilog(
            (context, loggerConfiguration) =>
                ConfigureLogger(loggerConfiguration, context.Configuration, configureSinks)
        );
    }

    /// <summary>
    /// Configures Serilog on a modern <see cref="IHostApplicationBuilder"/> (for example
    /// <c>Host.CreateApplicationBuilder()</c> or <c>WebApplication.CreateBuilder()</c>).
    /// </summary>
    public static TBuilder UseLogging<TBuilder>(
        this TBuilder builder,
        Action<LoggingSinkOptions>? configureSinks = null
    )
        where TBuilder : IHostApplicationBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddSerilog(
            (_, loggerConfiguration) =>
                ConfigureLogger(loggerConfiguration, builder.Configuration, configureSinks)
        );

        return builder;
    }

    private static void ConfigureLogger(
        LoggerConfiguration loggerConfiguration,
        IConfiguration configuration,
        Action<LoggingSinkOptions>? configureSinks
    )
    {
        SelfLog.Enable(Console.Error);

        loggerConfiguration.SetLogLevel(configuration.GetSection(LoggingConfiguration.RootSection));

        var options = new LoggingSinkOptions();

        try
        {
            configureSinks?.Invoke(options);

            foreach (var sink in options.LoggingSinks)
            {
                sink.Add(loggerConfiguration, configuration);
            }
        }
        catch (Exception exception)
        {
            SelfLog.WriteLine($"Exception during logging configuration: {exception.Message}");
        }
    }

    private static LoggerConfiguration SetLogLevel(
        this LoggerConfiguration configuration,
        IConfiguration loggingConfiguration
    )
    {
        var logLevels = loggingConfiguration
            .GetSection("LogLevel")
            .Get<Dictionary<string, string>>();

        if (logLevels is null || logLevels.Count == 0)
        {
            return configuration.SetDefaultLogLevel();
        }

        foreach (var (category, value) in logLevels)
        {
            if (!Enum.TryParse(value, out LogEventLevel level))
            {
                continue;
            }

            if (category == "Default")
            {
                configuration.MinimumLevel.Is(level);
            }
            else
            {
                configuration.MinimumLevel.Override(category, level);
            }
        }

        return configuration;
    }
}
