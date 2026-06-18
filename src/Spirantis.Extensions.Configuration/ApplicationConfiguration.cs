using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace Spirantis.Extensions.Configuration;

/// <summary>
/// Builds layered application configuration from environment variables, command-line
/// arguments, and a set of pluggable <see cref="IConfigurationSourceAgent"/> instances.
/// </summary>
public static class ApplicationConfiguration
{
    extension(IConfigurationBuilder configurationBuilder)
    {
        /// <summary>
        /// Adds environment variables and command-line arguments, then invokes each
        /// configured <see cref="IConfigurationSourceAgent"/> in order before applying
        /// command-line arguments once more so they take final precedence.
        /// </summary>
        /// <param name="commandLineSwitchMappings">Switch mappings for the command-line source.</param>
        /// <param name="commandLineArgs">The raw command-line arguments.</param>
        /// <param name="options">An optional callback to configure the source agents.</param>
        /// <returns>The same <see cref="IConfigurationBuilder"/> for chaining.</returns>
        public IConfigurationBuilder AddConfiguration(
            IDictionary<string, string> commandLineSwitchMappings,
            string[] commandLineArgs,
            Action<ConfigurationSourceOptions>? options = null
        )
        {
            ArgumentNullException.ThrowIfNull(configurationBuilder);

            configurationBuilder
                .AddEnvironmentVariables()
                .AddCommandLine(commandLineArgs, commandLineSwitchMappings);

            Debug.WriteLine(
                "Building environment configuration from environment variables and command line"
            );

            var baseConfiguration = configurationBuilder.Build();

            var configurationSourceOptions = new ConfigurationSourceOptions();
            options?.Invoke(configurationSourceOptions);

            configurationBuilder.SetBasePath(configurationSourceOptions.BaseDirectory);

            Debug.WriteLine(
                "Specified configuration source agents: {0}",
                configurationSourceOptions.ConfigurationSourceAgents.Count
            );

            foreach (var agent in configurationSourceOptions.ConfigurationSourceAgents)
            {
                Debug.WriteLine("Invoking configuration agent '{0}'", agent.GetType().FullName);

                try
                {
                    agent.Add(configurationBuilder, baseConfiguration);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(
                        "Exception thrown while invoking configuration agent '{0}': {1}",
                        agent.GetType().FullName,
                        exception.Message
                    );

                    throw;
                }
            }

            return configurationBuilder.AddCommandLine(commandLineArgs, commandLineSwitchMappings);
        }
    }
}
