using System.Reflection;
using System.Runtime.InteropServices;
using Serilog;
using Serilog.Configuration;
using Serilog.Formatting.Display;
using Serilog.Sinks.Syslog;

namespace Spirantis.Extensions.Logging.Sink.Syslog;

/// <summary>
/// Serilog configuration extensions for writing to the local syslog daemon.
/// </summary>
public static class SyslogLocalConfigurationExtensions
{
    /// <summary>
    /// Writes RFC 5424 messages to the local syslog daemon. Supported on Linux only.
    /// </summary>
    /// <param name="loggerSinkConfig">The sink configuration to extend.</param>
    /// <param name="facility">The syslog facility to log under.</param>
    /// <param name="outputTemplate">An optional message output template.</param>
    /// <returns>The logger configuration for chaining.</returns>
    public static LoggerConfiguration LocalRfc5424Syslog(
        this LoggerSinkConfiguration loggerSinkConfig,
        Facility facility = Facility.Local0,
        string? outputTemplate = null
    )
    {
        ArgumentNullException.ThrowIfNull(loggerSinkConfig);

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            throw new PlatformNotSupportedException(
                "The local syslog sink is only supported on Linux systems."
            );
        }

        var templateFormatter = string.IsNullOrWhiteSpace(outputTemplate)
            ? null
            : new MessageTemplateTextFormatter(outputTemplate, formatProvider: null);

        string applicationName = Assembly.GetEntryAssembly()?.GetName().Name ?? "application";
        var formatter = new Rfc5424Formatter(facility, applicationName, templateFormatter);

        var syslogService = new LocalSyslogService(facility);
        syslogService.Open();

        return loggerSinkConfig.Sink(new SyslogLocalSink(formatter, syslogService));
    }
}
