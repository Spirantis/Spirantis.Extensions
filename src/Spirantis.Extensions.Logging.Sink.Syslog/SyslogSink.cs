using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.Syslog;

namespace Spirantis.Extensions.Logging.Sink.Syslog;

/// <summary>
/// A logging sink that writes RFC 5424 messages to the local syslog daemon on Linux.
/// Enabled by <c>Logging:Syslog:Enabled</c>; the facility is configured via
/// <c>Logging:Syslog:FacilityCode</c>. No-op on non-Linux platforms.
/// </summary>
public sealed class SyslogSink : ILoggingSink
{
    private const string SyslogOutputTemplate = "{Message:lj}{NewLine}{Exception}";

    /// <inheritdoc />
    public void Add(LoggerConfiguration loggerConfiguration, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(loggerConfiguration);

        var section = configuration.GetEnabledSinkSection("Syslog");

        if (section is not null && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            loggerConfiguration.WriteTo.LocalRfc5424Syslog(
                facility: GetSyslogFacility(section),
                outputTemplate: SyslogOutputTemplate
            );
        }
    }

    private static Facility GetSyslogFacility(IConfiguration section)
    {
        int facilityCode = section.GetValue("FacilityCode", (int)Facility.Local5);
        return Enum.IsDefined((Facility)facilityCode) ? (Facility)facilityCode : Facility.Local5;
    }
}
