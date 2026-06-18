using Microsoft.Extensions.Configuration;

namespace Spirantis.Extensions.Configuration;

/// <summary>
/// Convenience accessors for the well-known <c>Environment</c> configuration section.
/// </summary>
public static class EnvironmentSettings
{
    extension(IConfiguration configuration)
    {
        /// <summary>
        /// Gets the configured environment name (<c>Environment:Name</c>),
        /// or <c>"undefined"</c> when it is not set.
        /// </summary>
        public string GetEnvironmentName() =>
            configuration.GetValue<string>("Environment:Name") ?? "undefined";

        /// <summary>
        /// Gets the environment prefix to apply, which is the environment name when
        /// <c>Environment:UseEnvPrefix</c> is enabled, otherwise <see langword="null"/>.
        /// </summary>
        public string? GetEnvironmentPrefix() =>
            configuration.GetValue("Environment:UseEnvPrefix", false)
                ? configuration.GetEnvironmentName()
                : null;
    }
}
