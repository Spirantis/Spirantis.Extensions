using Microsoft.Extensions.Configuration;

namespace Spirantis.Extensions.Configuration.Tests;

public sealed class EnvironmentSettingsTests
{
    private static IConfiguration BuildConfiguration(params (string Key, string Value)[] values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(
                values.Select(value => new KeyValuePair<string, string?>(value.Key, value.Value))
            )
            .Build();

    [Fact]
    public void GetEnvironmentName_ReturnsConfiguredValue()
    {
        var configuration = BuildConfiguration(("Environment:Name", "production"));

        Assert.Equal("production", configuration.GetEnvironmentName());
    }

    [Fact]
    public void GetEnvironmentName_DefaultsToUndefined_WhenNotSet()
    {
        var configuration = BuildConfiguration();

        Assert.Equal("undefined", configuration.GetEnvironmentName());
    }

    [Fact]
    public void GetEnvironmentPrefix_WhenEnabled_ReturnsEnvironmentName()
    {
        var configuration = BuildConfiguration(
            ("Environment:Name", "staging"),
            ("Environment:UseEnvPrefix", "true")
        );

        Assert.Equal("staging", configuration.GetEnvironmentPrefix());
    }

    [Fact]
    public void GetEnvironmentPrefix_WhenDisabled_ReturnsNull()
    {
        var configuration = BuildConfiguration(
            ("Environment:Name", "staging"),
            ("Environment:UseEnvPrefix", "false")
        );

        Assert.Null(configuration.GetEnvironmentPrefix());
    }

    [Fact]
    public void GetEnvironmentPrefix_WhenAbsent_ReturnsNull()
    {
        var configuration = BuildConfiguration(("Environment:Name", "staging"));

        Assert.Null(configuration.GetEnvironmentPrefix());
    }
}
