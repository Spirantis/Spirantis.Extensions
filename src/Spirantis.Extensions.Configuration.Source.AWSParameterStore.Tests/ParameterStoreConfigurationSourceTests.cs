using Amazon.Extensions.NETCore.Setup;
using Microsoft.Extensions.Configuration;

namespace Spirantis.Extensions.Configuration.Source.AWSParameterStore.Tests;

public sealed class ParameterStoreConfigurationSourceTests
{
    [Fact]
    public void Defaults_AreSet()
    {
        var source = new ParameterStoreConfigurationSource();

        Assert.Empty(source.Filters);
        Assert.False(source.Optional);
        Assert.Null(source.Prefix);
        Assert.Null(source.ReloadAfter);
    }

    [Fact]
    public void Build_ReturnsParameterStoreConfigurationProvider()
    {
        var source = new ParameterStoreConfigurationSource
        {
            AwsOptions = new AWSOptions(),
            BasePath = "/app",
        };

        var provider = source.Build(new ConfigurationBuilder());

        Assert.IsType<ParameterStoreConfigurationProvider>(provider);
    }
}
