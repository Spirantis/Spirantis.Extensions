using Amazon.Extensions.NETCore.Setup;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Configuration;

namespace Spirantis.Extensions.Configuration.Source.AWSParameterStore;

/// <summary>
/// An <see cref="IConfigurationSource"/> backed by AWS Systems Manager Parameter Store.
/// </summary>
public class ParameterStoreConfigurationSource : IConfigurationSource
{
    /// <summary>AWS client options used to create the Systems Management client.</summary>
    public AWSOptions? AwsOptions { get; set; }

    /// <summary>The parameter path prefix to load recursively.</summary>
    public string? BasePath { get; set; }

    /// <summary>Optional filters applied to the parameter query.</summary>
    public List<ParameterStringFilter> Filters { get; } = [];

    /// <summary>Optional handler invoked when loading parameters fails.</summary>
    public Action<ParameterStoreExceptionContext>? OnLoadException { get; set; }

    /// <summary>Whether load failures are tolerated.</summary>
    public bool Optional { get; set; }

    /// <summary>An optional prefix prepended to every produced configuration key.</summary>
    public string? Prefix { get; set; }

    /// <summary>When set, parameters are reloaded on this interval.</summary>
    public TimeSpan? ReloadAfter { get; set; }

    /// <inheritdoc />
    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new ParameterStoreConfigurationProvider(this);
}
