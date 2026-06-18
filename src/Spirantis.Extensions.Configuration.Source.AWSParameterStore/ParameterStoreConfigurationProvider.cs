using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Spirantis.Extensions.Configuration.Source.AWSParameterStore;

/// <summary>
/// A <see cref="ConfigurationProvider"/> that loads its data from AWS Systems Manager
/// Parameter Store, with optional periodic reload and exception handling.
/// </summary>
public class ParameterStoreConfigurationProvider : ConfigurationProvider
{
    public ParameterStoreConfigurationProvider(ParameterStoreConfigurationSource source)
        : this(source, new ParameterStoreProcessor(source)) { }

    public ParameterStoreConfigurationProvider(
        ParameterStoreConfigurationSource source,
        IParameterStoreProcessor processor
    )
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(processor);

        Source = source;
        Processor = processor;

        if (source.AwsOptions is null)
        {
            throw new ArgumentException("AwsOptions must be set.", nameof(source));
        }

        if (source.BasePath is null)
        {
            throw new ArgumentException("BasePath must be set.", nameof(source));
        }

        if (source.ReloadAfter is not null)
        {
            ChangeToken.OnChange(
                () =>
                {
                    var cancellationTokenSource = new CancellationTokenSource(
                        source.ReloadAfter.Value
                    );
                    return new CancellationChangeToken(cancellationTokenSource.Token);
                },
                () => LoadAsync(true).ConfigureAwait(false).GetAwaiter().GetResult()
            );
        }
    }

    /// <summary>The source that created this provider.</summary>
    public ParameterStoreConfigurationSource Source { get; }

    private IParameterStoreProcessor Processor { get; }

    /// <inheritdoc />
    public override void Load() => LoadAsync(false).ConfigureAwait(false).GetAwaiter().GetResult();

    private static bool CompareDictionaries<TKey, TValue>(
        IDictionary<TKey, TValue> first,
        IDictionary<TKey, TValue> second
    )
    {
        if (ReferenceEquals(first, second))
        {
            return true;
        }

        if (first.Count != second.Count)
        {
            return false;
        }

        foreach (var pair in first)
        {
            if (!second.TryGetValue(pair.Key, out var secondValue))
            {
                return false;
            }

            if (!EqualityComparer<TValue>.Default.Equals(pair.Value, secondValue))
            {
                return false;
            }
        }

        return true;
    }

    // When a reload fails and no OnLoadException handler is set, the exception is ignored.
    private async Task LoadAsync(bool reload)
    {
        try
        {
            var newData =
                await Processor.GetDataAsync().ConfigureAwait(false)
                ?? new Dictionary<string, string?>();

            if (!CompareDictionaries(Data, newData))
            {
                Data = newData;
                OnReload();
            }
        }
        catch (Exception exception)
        {
            if (Source.Optional)
            {
                return;
            }

            bool ignoreException = reload;

            if (Source.OnLoadException is not null)
            {
                var exceptionContext = new ParameterStoreExceptionContext
                {
                    Provider = this,
                    Exception = exception,
                    Reload = reload,
                };

                Source.OnLoadException(exceptionContext);
                ignoreException = exceptionContext.Ignore;
            }

            if (!ignoreException)
            {
                throw;
            }
        }
    }
}
