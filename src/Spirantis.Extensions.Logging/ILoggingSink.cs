using Microsoft.Extensions.Configuration;
using Serilog;

namespace Spirantis.Extensions.Logging;

/// <summary>
/// A pluggable logging sink that contributes its output to a Serilog
/// <see cref="LoggerConfiguration"/>, reading its own settings from configuration.
/// </summary>
public interface ILoggingSink
{
    /// <summary>
    /// Adds this sink to <paramref name="loggerConfiguration"/>.
    /// </summary>
    /// <param name="loggerConfiguration">The Serilog logger configuration to contribute to.</param>
    /// <param name="configuration">Application configuration used to read the sink's settings.</param>
    void Add(LoggerConfiguration loggerConfiguration, IConfiguration configuration);
}
