namespace Spirantis.Extensions.Logging;

/// <summary>
/// Options controlling which logging sinks <see cref="ApplicationLogging"/> configures.
/// </summary>
public sealed class LoggingSinkOptions
{
    /// <summary>The default Serilog output template shared by the text-based sinks.</summary>
    public const string StandardOutputTemplate =
        "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}";

    /// <summary>The ordered set of sinks to configure.</summary>
    public IList<ILoggingSink> LoggingSinks { get; set; } = [];
}
