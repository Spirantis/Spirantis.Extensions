using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Configuration;

namespace Spirantis.Extensions.Configuration.Source.AWSParameterStore;

/// <summary>
/// Default <see cref="IParameterStoreProcessor"/> that queries AWS Systems Manager
/// Parameter Store recursively, flattening JSON parameter values into configuration keys.
/// </summary>
public class ParameterStoreProcessor : IParameterStoreProcessor
{
    public ParameterStoreProcessor(ParameterStoreConfigurationSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        Source = source;
    }

    private ParameterStoreConfigurationSource Source { get; }

    /// <summary>Prepends <paramref name="prefix"/> to every key in <paramref name="input"/>.</summary>
    public static IDictionary<string, string?> AddPrefix(
        IDictionary<string, string?> input,
        string? prefix
    )
    {
        ArgumentNullException.ThrowIfNull(input);

        return string.IsNullOrEmpty(prefix)
            ? input
            : input.ToDictionary(
                pair => $"{prefix}{ConfigurationPath.KeyDelimiter}{pair.Key}",
                pair => pair.Value,
                StringComparer.OrdinalIgnoreCase
            );
    }

    /// <summary>
    /// Flattens each parameter's JSON value into configuration keys, falling back to the
    /// raw value when a parameter is not valid JSON.
    /// </summary>
    public static IDictionary<string, string?> ProcessParameters(
        IEnumerable<Parameter> parameters,
        string basePath
    )
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(basePath);

        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var parameter in parameters)
        {
            string name = parameter.Name ?? string.Empty;
            string? value = parameter.Value;

            try
            {
                var parsed = JsonConfigurationParser.Parse(value ?? string.Empty, name);

                foreach (var pair in parsed)
                {
                    result[GetKey(pair.Key, basePath)] = pair.Value;
                }
            }
            catch (Exception)
            {
                result[GetKey(name, basePath)] = value;
            }
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<IDictionary<string, string?>> GetDataAsync()
    {
        using var client = Source.AwsOptions!.CreateServiceClient<IAmazonSimpleSystemsManagement>();

        var parameters = new List<Parameter>();
        string? nextToken = null;

        do
        {
            var response = await client
                .GetParametersByPathAsync(
                    new GetParametersByPathRequest
                    {
                        Path = Source.BasePath,
                        Recursive = true,
                        WithDecryption = true,
                        NextToken = nextToken,
                        ParameterFilters = Source.Filters,
                    }
                )
                .ConfigureAwait(false);

            nextToken = response.NextToken;

            if (response.Parameters is not null)
            {
                parameters.AddRange(response.Parameters);
            }
        } while (!string.IsNullOrEmpty(nextToken));

        return AddPrefix(ProcessParameters(parameters, Source.BasePath!), Source.Prefix);
    }

    private static string GetKey(string parameterName, string path)
    {
        string name = parameterName.StartsWith(path, StringComparison.OrdinalIgnoreCase)
            ? parameterName[path.Length..]
            : parameterName;

        return name.TrimStart('/')
            .Replace("/", ConfigurationPath.KeyDelimiter, StringComparison.Ordinal);
    }
}
