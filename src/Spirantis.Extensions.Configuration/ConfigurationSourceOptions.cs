namespace Spirantis.Extensions.Configuration;

/// <summary>
/// Options controlling how <see cref="ApplicationConfiguration"/> assembles
/// configuration sources.
/// </summary>
public class ConfigurationSourceOptions
{
    /// <summary>
    /// The base directory used to resolve relative configuration file paths.
    /// Defaults to the application's base directory.
    /// </summary>
    public string BaseDirectory { get; set; } = AppContext.BaseDirectory;

    /// <summary>
    /// The ordered set of configuration source agents to invoke.
    /// </summary>
    public IList<IConfigurationSourceAgent> ConfigurationSourceAgents { get; set; } = [];
}
