using Microsoft.Extensions.Configuration;

namespace Spirantis.Extensions.Configuration.Source.Json.Tests;

public sealed class ServiceJsonFileSourceAgentTests
{
    private static IConfiguration BuildEnvironment(params (string Key, string Value)[] values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(
                values.Select(value => new KeyValuePair<string, string?>(value.Key, value.Value))
            )
            .Build();

    [Fact]
    public void Defaults_AreSet()
    {
        var agent = new ServiceJsonFileSourceAgent();

        Assert.Equal("SPIRANTIS_CONFIG_JSON_SERVICE_ENABLE", agent.EnableEnvironmentKey);
        Assert.Equal("config", agent.FileName);
        Assert.True(agent.IsOptional);
        Assert.True(agent.ReloadOnChange);
    }

    [Fact]
    public void Add_WhenEnabled_LoadsServiceConfigFile()
    {
        using var temp = new TempDirectory();
        temp.WriteConfig("config.json", """{ "Service": { "Name": "svc" } }""");

        var builder = new ConfigurationBuilder();
        builder.SetBasePath(temp.Path);

        new ServiceJsonFileSourceAgent().Add(
            builder,
            BuildEnvironment(("SPIRANTIS_CONFIG_JSON_SERVICE_ENABLE", "true"))
        );

        var configuration = builder.Build();
        Assert.Equal("svc", configuration["Service:Name"]);
    }

    [Fact]
    public void Add_WhenDisabled_DoesNotLoadFile()
    {
        using var temp = new TempDirectory();
        temp.WriteConfig("config.json", """{ "Service": { "Name": "svc" } }""");

        var builder = new ConfigurationBuilder();
        builder.SetBasePath(temp.Path);

        new ServiceJsonFileSourceAgent().Add(builder, BuildEnvironment());

        Assert.Empty(
            builder.Sources.OfType<Microsoft.Extensions.Configuration.Json.JsonConfigurationSource>()
        );
    }

    [Fact]
    public void Add_WithNullBuilder_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ServiceJsonFileSourceAgent().Add(null!, BuildEnvironment())
        );
    }
}
