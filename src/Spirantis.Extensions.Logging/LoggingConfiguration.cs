using Microsoft.Extensions.Configuration;

namespace Spirantis.Extensions.Logging;

/// <summary>
/// Helpers for the shared logging configuration convention, under which every sink reads
/// its settings from <c>Logging:&lt;SinkName&gt;</c> and activates on an <c>Enabled</c> flag.
/// </summary>
public static class LoggingConfiguration
{
    /// <summary>The root configuration section under which logging settings live.</summary>
    public const string RootSection = "Logging";

    /// <summary>
    /// Returns the <c>Logging:<paramref name="sinkName"/></c> section when its <c>Enabled</c>
    /// value is <see langword="true"/>, otherwise <see langword="null"/>.
    /// </summary>
    public static IConfigurationSection? GetEnabledSinkSection(
        this IConfiguration configuration,
        string sinkName
    )
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var section = configuration.GetSection($"{RootSection}:{sinkName}");
        return section.GetValue("Enabled", false) ? section : null;
    }
}
