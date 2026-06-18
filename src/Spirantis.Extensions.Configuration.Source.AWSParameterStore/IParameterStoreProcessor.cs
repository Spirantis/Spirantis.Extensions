namespace Spirantis.Extensions.Configuration.Source.AWSParameterStore;

/// <summary>
/// Retrieves and flattens AWS Systems Manager Parameter Store data into
/// configuration key/value pairs.
/// </summary>
public interface IParameterStoreProcessor
{
    /// <summary>Loads the parameter data as flattened configuration keys.</summary>
    Task<IDictionary<string, string?>> GetDataAsync();
}
