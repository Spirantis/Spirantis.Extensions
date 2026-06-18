using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Spirantis.Extensions.Configuration.Source.Json;

/// <summary>
/// A configuration source agent that adds a machine-wide JSON file, searching a
/// configurable list of candidate directories and loading the file from the first
/// one that exists, when its enabling environment key is set.
/// </summary>
public class GlobalJsonFileSourceAgent : IConfigurationSourceAgent
{
    /// <summary>
    /// The environment key that enables this agent. Defaults to
    /// <c>SPIRANTIS_CONFIG_JSON_GLOBAL_ENABLE</c>.
    /// </summary>
    public string EnableEnvironmentKey { get; set; } = "SPIRANTIS_CONFIG_JSON_GLOBAL_ENABLE";

    /// <summary>The file name (without extension) to load. Defaults to <c>config</c>.</summary>
    public string FileName { get; set; } = "config";

    /// <summary>Whether a missing file is tolerated. Defaults to <see langword="true"/>.</summary>
    public bool IsOptional { get; set; } = true;

    /// <summary>
    /// Candidate directories searched in order; the first existing directory is used.
    /// </summary>
    public List<string> RelativeConfigurationPaths { get; set; } =
    [
        "/etc/spirantis/",
        "/opt/spirantis/etc",
        "../Configuration",
        "../../Configuration",
        "../../../Configuration",
        "../../../../Configuration",
    ];

    /// <summary>Whether the file is reloaded on change. Defaults to <see langword="true"/>.</summary>
    public bool ReloadOnChange { get; set; } = true;

    private PhysicalFileProvider CreateSystemConfigurationFileProvider()
    {
        foreach (string relativePath in RelativeConfigurationPaths)
        {
            string absolutePath = Path.GetFullPath(relativePath);

            if (Directory.Exists(absolutePath))
            {
                return new PhysicalFileProvider(absolutePath);
            }
        }

        throw new DirectoryNotFoundException("Configuration directory not found");
    }

    /// <inheritdoc />
    public void Add(IConfigurationBuilder builder, IConfiguration environmentConfiguration)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(environmentConfiguration);

        if (environmentConfiguration.GetValue(EnableEnvironmentKey, false))
        {
            builder.AddJsonFile(
                CreateSystemConfigurationFileProvider(),
                $"{FileName}.json",
                IsOptional,
                ReloadOnChange
            );
        }
    }

    /// <summary>Appends an additional candidate directory to search.</summary>
    public void AddRelativeConfigurationPath(string relativeConfigurationPath) =>
        RelativeConfigurationPaths.Add(relativeConfigurationPath);

    /// <summary>Replaces the candidate directories with a single path.</summary>
    public void SetRelativeConfigurationPath(string relativeConfigurationPath) =>
        RelativeConfigurationPaths = [relativeConfigurationPath];
}
