using Microsoft.Extensions.Configuration;

namespace Spirantis.Extensions.Configuration.Source.Json.Tests;

public sealed class GlobalJsonFileSourceAgentTests
{
    private static IConfiguration BuildEnvironment(params (string Key, string Value)[] values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(
                values.Select(value => new KeyValuePair<string, string?>(value.Key, value.Value))
            )
            .Build();

    [Fact]
    public void Defaults_AreSet_AndOriginNeutral()
    {
        var agent = new GlobalJsonFileSourceAgent();

        Assert.Equal("SPIRANTIS_CONFIG_JSON_GLOBAL_ENABLE", agent.EnableEnvironmentKey);
        Assert.Equal("config", agent.FileName);
        Assert.True(agent.IsOptional);
        Assert.True(agent.ReloadOnChange);
        Assert.Equal(
            [
                "/etc/spirantis/",
                "/opt/spirantis/etc",
                "../Configuration",
                "../../Configuration",
                "../../../Configuration",
                "../../../../Configuration",
            ],
            agent.RelativeConfigurationPaths
        );
    }

    [Fact]
    public void SetRelativeConfigurationPath_ReplacesAllPaths()
    {
        var agent = new GlobalJsonFileSourceAgent();

        agent.SetRelativeConfigurationPath("/only/path");

        Assert.Equal(["/only/path"], agent.RelativeConfigurationPaths);
    }

    [Fact]
    public void AddRelativeConfigurationPath_AppendsPath()
    {
        var agent = new GlobalJsonFileSourceAgent();
        int originalCount = agent.RelativeConfigurationPaths.Count;

        agent.AddRelativeConfigurationPath("/extra/path");

        Assert.Equal(originalCount + 1, agent.RelativeConfigurationPaths.Count);
        Assert.Contains("/extra/path", agent.RelativeConfigurationPaths);
    }

    [Fact]
    public void Add_WhenEnabled_LoadsFromFirstExistingPath()
    {
        using var temp = new TempDirectory();
        temp.WriteConfig("config.json", """{ "Global": { "Flag": "on" } }""");

        var agent = new GlobalJsonFileSourceAgent();
        agent.SetRelativeConfigurationPath(temp.Path);

        var builder = new ConfigurationBuilder();
        agent.Add(builder, BuildEnvironment(("SPIRANTIS_CONFIG_JSON_GLOBAL_ENABLE", "true")));

        var configuration = builder.Build();
        Assert.Equal("on", configuration["Global:Flag"]);
    }

    [Fact]
    public void Add_WhenEnabled_AndNoCandidatePathExists_Throws()
    {
        var agent = new GlobalJsonFileSourceAgent();
        agent.SetRelativeConfigurationPath(
            Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}")
        );

        var builder = new ConfigurationBuilder();

        Assert.Throws<DirectoryNotFoundException>(() =>
            agent.Add(builder, BuildEnvironment(("SPIRANTIS_CONFIG_JSON_GLOBAL_ENABLE", "true")))
        );
    }

    [Fact]
    public void Add_WhenDisabled_DoesNothing()
    {
        var agent = new GlobalJsonFileSourceAgent();
        agent.SetRelativeConfigurationPath(
            Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}")
        );

        var builder = new ConfigurationBuilder();
        agent.Add(builder, BuildEnvironment());

        Assert.Empty(builder.Sources);
    }
}
