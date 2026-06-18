namespace Spirantis.Extensions.Configuration.Source.AWSParameterStore;

/// <summary>
/// Context passed to the load-exception handler of a
/// <see cref="ParameterStoreConfigurationProvider"/>.
/// </summary>
public sealed class ParameterStoreExceptionContext
{
    /// <summary>The exception raised while loading parameters.</summary>
    public Exception? Exception { get; set; }

    /// <summary>Set to <see langword="true"/> to swallow the exception.</summary>
    public bool Ignore { get; set; }

    /// <summary>The provider that raised the exception.</summary>
    public ParameterStoreConfigurationProvider? Provider { get; set; }

    /// <summary>Whether the failure occurred during a reload rather than the initial load.</summary>
    public bool Reload { get; set; }
}
