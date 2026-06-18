using Amazon.Extensions.NETCore.Setup;

namespace Spirantis.Extensions.Configuration.Source.AWSParameterStore.Tests;

public sealed class ParameterStoreConfigurationProviderTests
{
    private sealed class StubProcessor : IParameterStoreProcessor
    {
        public IDictionary<string, string?> Data { get; set; } = new Dictionary<string, string?>();

        public Exception? ToThrow { get; set; }

        public Task<IDictionary<string, string?>> GetDataAsync()
        {
            if (ToThrow is not null)
            {
                throw ToThrow;
            }

            return Task.FromResult(Data);
        }
    }

    private static ParameterStoreConfigurationSource ValidSource(
        bool optional = false,
        Action<ParameterStoreExceptionContext>? onLoadException = null
    ) =>
        new()
        {
            AwsOptions = new AWSOptions(),
            BasePath = "/app",
            Optional = optional,
            OnLoadException = onLoadException,
        };

    [Fact]
    public void Load_PopulatesData_FromProcessor()
    {
        var processor = new StubProcessor
        {
            Data = new Dictionary<string, string?> { ["Key"] = "Value" },
        };
        var provider = new ParameterStoreConfigurationProvider(ValidSource(), processor);

        provider.Load();

        Assert.True(provider.TryGet("Key", out string? value));
        Assert.Equal("Value", value);
    }

    [Fact]
    public void Constructor_WithNullSource_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ParameterStoreConfigurationProvider(null!, new StubProcessor())
        );
    }

    [Fact]
    public void Constructor_WithoutAwsOptions_Throws()
    {
        var source = new ParameterStoreConfigurationSource { BasePath = "/app" };

        Assert.Throws<ArgumentException>(() =>
            new ParameterStoreConfigurationProvider(source, new StubProcessor())
        );
    }

    [Fact]
    public void Constructor_WithoutBasePath_Throws()
    {
        var source = new ParameterStoreConfigurationSource { AwsOptions = new AWSOptions() };

        Assert.Throws<ArgumentException>(() =>
            new ParameterStoreConfigurationProvider(source, new StubProcessor())
        );
    }

    [Fact]
    public void Load_WhenOptional_SwallowsProcessorException()
    {
        var processor = new StubProcessor { ToThrow = new InvalidOperationException("boom") };
        var provider = new ParameterStoreConfigurationProvider(
            ValidSource(optional: true),
            processor
        );

        provider.Load();

        Assert.False(provider.TryGet("Anything", out _));
    }

    [Fact]
    public void Load_WhenNotOptional_RethrowsProcessorException()
    {
        var processor = new StubProcessor { ToThrow = new InvalidOperationException("boom") };
        var provider = new ParameterStoreConfigurationProvider(
            ValidSource(optional: false),
            processor
        );

        Assert.Throws<InvalidOperationException>(provider.Load);
    }

    [Fact]
    public void Load_WhenHandlerIgnoresException_DoesNotRethrow()
    {
        ParameterStoreExceptionContext? captured = null;
        var source = ValidSource(
            optional: false,
            onLoadException: context =>
            {
                captured = context;
                context.Ignore = true;
            }
        );
        var processor = new StubProcessor { ToThrow = new InvalidOperationException("boom") };
        var provider = new ParameterStoreConfigurationProvider(source, processor);

        provider.Load();

        Assert.NotNull(captured);
        Assert.Same(provider, captured!.Provider);
        Assert.False(captured.Reload);
    }
}
