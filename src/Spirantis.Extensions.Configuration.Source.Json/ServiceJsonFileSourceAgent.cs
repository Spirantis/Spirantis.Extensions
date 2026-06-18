using Microsoft.Extensions.Configuration;

namespace Spirantis.Extensions.Configuration.Source.Json;

/// <summary>
/// A configuration source agent that adds a service-local JSON file
/// (resolved relative to the configuration builder's base path) when its
/// enabling environment key is set.
/// </summary>
public sealed class ServiceJsonFileSourceAgent : IConfigurationSourceAgent
{
    /// <summary>
    /// The environment key that enables this agent. Defaults to
    /// <c>SPIRANTIS_CONFIG_JSON_SERVICE_ENABLE</c>.
    /// </summary>
    public string EnableEnvironmentKey { get; set; } = "SPIRANTIS_CONFIG_JSON_SERVICE_ENABLE";

    /// <summary>The file name (without extension) to load. Defaults to <c>config</c>.</summary>
    public string FileName { get; set; } = "config";

    /// <summary>Whether a missing file is tolerated. Defaults to <see langword="true"/>.</summary>
    public bool IsOptional { get; set; } = true;

    /// <summary>Whether the file is reloaded on change. Defaults to <see langword="true"/>.</summary>
    public bool ReloadOnChange { get; set; } = true;

    /// <inheritdoc />
    public void Add(IConfigurationBuilder builder, IConfiguration environmentConfiguration)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(environmentConfiguration);

        if (environmentConfiguration.GetValue(EnableEnvironmentKey, false))
        {
            builder.AddJsonFile($"{FileName}.json", IsOptional, ReloadOnChange);
        }
    }
}
