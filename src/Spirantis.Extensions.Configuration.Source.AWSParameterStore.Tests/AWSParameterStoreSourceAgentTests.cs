using Amazon;
using Microsoft.Extensions.Configuration;

namespace Spirantis.Extensions.Configuration.Source.AWSParameterStore.Tests;

public sealed class AWSParameterStoreSourceAgentTests
{
    private static IConfiguration BuildEnvironment(params (string Key, string Value)[] values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(
                values.Select(value => new KeyValuePair<string, string?>(value.Key, value.Value))
            )
            .Build();

    [Fact]
    public void Defaults_AreSpirantisPrefixed()
    {
        var agent = new AWSParameterStoreSourceAgent();

        Assert.Equal("SPIRANTIS_CONFIG_AWS_PS_ENABLE", agent.EnableEnvironmentKey);
        Assert.Equal("SPIRANTIS_CONFIG_AWS_PS_PATH", agent.BaseParameterPathEnvironmentKey);
        Assert.Equal("SPIRANTIS_CONFIG_AWS_PS_REGION", agent.AWSRegionEnvironmentKey);
        Assert.True(agent.IsOptional);
    }

    [Fact]
    public void Add_WhenDisabled_AddsNoSources()
    {
        var builder = new ConfigurationBuilder();

        new AWSParameterStoreSourceAgent().Add(builder, BuildEnvironment());

        Assert.Empty(builder.Sources);
    }

    [Fact]
    public void Add_WhenEnabled_AddsConfiguredSource()
    {
        var builder = new ConfigurationBuilder();

        new AWSParameterStoreSourceAgent().Add(
            builder,
            BuildEnvironment(
                ("SPIRANTIS_CONFIG_AWS_PS_ENABLE", "true"),
                ("SPIRANTIS_CONFIG_AWS_PS_PATH", "/app"),
                ("SPIRANTIS_CONFIG_AWS_PS_REGION", "eu-west-1")
            )
        );

        var source = Assert.IsType<ParameterStoreConfigurationSource>(
            Assert.Single(builder.Sources)
        );
        Assert.Equal("/app", source.BasePath);
        Assert.True(source.Optional);
        Assert.Equal(RegionEndpoint.GetBySystemName("eu-west-1"), source.AwsOptions!.Region);
    }

    [Fact]
    public void Add_WhenEnabled_WithBlankBasePath_AddsNoSources()
    {
        var builder = new ConfigurationBuilder();

        new AWSParameterStoreSourceAgent().Add(
            builder,
            BuildEnvironment(("SPIRANTIS_CONFIG_AWS_PS_ENABLE", "true"))
        );

        Assert.Empty(builder.Sources);
    }

    [Fact]
    public void Add_WhenEnabled_AddsOneSourcePerBasePathKey()
    {
        var builder = new ConfigurationBuilder();

        new AWSParameterStoreSourceAgent().Add(
            builder,
            BuildEnvironment(
                ("SPIRANTIS_CONFIG_AWS_PS_ENABLE", "true"),
                ("SPIRANTIS_CONFIG_AWS_PS_PATH", "/app"),
                ("SPIRANTIS_CONFIG_AWS_PS_PATH_SHARED", "/shared")
            )
        );

        var basePaths = builder
            .Sources.OfType<ParameterStoreConfigurationSource>()
            .Select(source => source.BasePath)
            .ToList();

        Assert.Equal(2, basePaths.Count);
        Assert.Contains("/app", basePaths);
        Assert.Contains("/shared", basePaths);
    }

    [Fact]
    public void Add_WithNullBuilder_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new AWSParameterStoreSourceAgent().Add(null!, BuildEnvironment())
        );
    }
}
