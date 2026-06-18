using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;

namespace Spirantis.Extensions.Configuration.Source.AWSParameterStore;

/// <summary>
/// A configuration source agent that loads parameters from AWS Systems Manager
/// Parameter Store for every base path discovered in the environment configuration,
/// when its enabling environment key is set. All environment keys are configurable.
/// </summary>
public sealed class AWSParameterStoreSourceAgent : IConfigurationSourceAgent
{
    /// <summary>Environment key for the optional custom AWS service endpoint.</summary>
    public string AWSEndpointEnvironmentKey { get; set; } = "SPIRANTIS_CONFIG_AWS_ENDPOINT";

    /// <summary>Environment key that, when set, enables verbose AWS SDK logging.</summary>
    public string AWSLoggingEnvironmentKey { get; set; } = "SPIRANTIS_CONFIG_AWS_LOGGING";

    /// <summary>Environment key for the AWS access key.</summary>
    public string AWSAccessKeyEnvironmentKey { get; set; } = "SPIRANTIS_CONFIG_AWS_PS_ACCKEY";

    /// <summary>Environment key for the AWS region system name.</summary>
    public string AWSRegionEnvironmentKey { get; set; } = "SPIRANTIS_CONFIG_AWS_PS_REGION";

    /// <summary>Environment key for the AWS secret key.</summary>
    public string AWSSecretKeyEnvironmentKey { get; set; } = "SPIRANTIS_CONFIG_AWS_PS_SECKEY";

    /// <summary>Environment key prefix identifying the parameter base path(s) to load.</summary>
    public string BaseParameterPathEnvironmentKey { get; set; } = "SPIRANTIS_CONFIG_AWS_PS_PATH";

    /// <summary>Environment key that enables this agent.</summary>
    public string EnableEnvironmentKey { get; set; } = "SPIRANTIS_CONFIG_AWS_PS_ENABLE";

    /// <summary>Whether load failures are tolerated. Defaults to <see langword="true"/>.</summary>
    public bool IsOptional { get; set; } = true;

    /// <summary>An optional prefix prepended to every produced configuration key.</summary>
    public string? Prefix { get; set; }

    /// <summary>When set, parameters are reloaded on this interval.</summary>
    public TimeSpan? ReloadAfter { get; set; }

    /// <summary>Optional handler invoked when loading parameters fails.</summary>
    public Action<ParameterStoreExceptionContext>? OnLoadException { get; set; }

    /// <inheritdoc />
    public void Add(IConfigurationBuilder builder, IConfiguration environmentConfiguration)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(environmentConfiguration);

        if (!environmentConfiguration.GetValue(EnableEnvironmentKey, false))
        {
            return;
        }

        var basePathKeys = environmentConfiguration
            .AsEnumerable()
            .Where(pair =>
                pair.Key.Contains(BaseParameterPathEnvironmentKey, StringComparison.Ordinal)
            )
            .Select(pair => pair.Key);

        foreach (string basePathKey in basePathKeys)
        {
            string? basePath = environmentConfiguration.GetValue<string>(basePathKey);

            if (!string.IsNullOrWhiteSpace(basePath))
            {
                AddForBasePath(builder, environmentConfiguration, basePath);
            }
        }
    }

    private void AddForBasePath(
        IConfigurationBuilder builder,
        IConfiguration environmentConfiguration,
        string basePath
    )
    {
        var awsOptions = new AWSOptions();

        string? awsEndpoint = environmentConfiguration.GetValue<string>(AWSEndpointEnvironmentKey);
        string? awsLogging = environmentConfiguration.GetValue<string>(AWSLoggingEnvironmentKey);
        string? accessKey = environmentConfiguration.GetValue<string>(AWSAccessKeyEnvironmentKey);
        string? secretKey = environmentConfiguration.GetValue<string>(AWSSecretKeyEnvironmentKey);
        string? awsRegion = environmentConfiguration.GetValue<string>(AWSRegionEnvironmentKey);

        if (!string.IsNullOrWhiteSpace(awsLogging))
        {
            AWSConfigs.LoggingConfig.LogTo =
                LoggingOptions.SystemDiagnostics | LoggingOptions.Console;
            AWSConfigs.LoggingConfig.LogResponses = ResponseLoggingOption.Always;
        }

        if (!string.IsNullOrWhiteSpace(awsEndpoint))
        {
            awsOptions.DefaultClientConfig.ServiceURL = awsEndpoint;
        }

        if (!string.IsNullOrWhiteSpace(accessKey) && !string.IsNullOrWhiteSpace(secretKey))
        {
            awsOptions.Credentials = new BasicAWSCredentials(accessKey, secretKey);
        }

        if (!string.IsNullOrWhiteSpace(awsRegion))
        {
            awsOptions.Region = RegionEndpoint.GetBySystemName(awsRegion);
        }

        builder.Add(
            new ParameterStoreConfigurationSource
            {
                AwsOptions = awsOptions,
                BasePath = basePath,
                ReloadAfter = ReloadAfter,
                Optional = IsOptional,
                Prefix = Prefix,
                OnLoadException = OnLoadException,
            }
        );
    }
}
