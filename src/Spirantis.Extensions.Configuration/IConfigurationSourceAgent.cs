using Microsoft.Extensions.Configuration;

namespace Spirantis.Extensions.Configuration;

/// <summary>
/// A pluggable source of configuration that contributes one or more
/// <see cref="IConfigurationSource"/> entries to an <see cref="IConfigurationBuilder"/>,
/// using an already-built environment configuration to decide what to add.
/// </summary>
public interface IConfigurationSourceAgent
{
    /// <summary>
    /// Adds this agent's configuration sources to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The configuration builder to contribute to.</param>
    /// <param name="environmentConfiguration">
    /// Configuration built from environment variables and command-line arguments,
    /// used to read the agent's own settings.
    /// </param>
    void Add(IConfigurationBuilder builder, IConfiguration environmentConfiguration);
}
