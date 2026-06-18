using Microsoft.Extensions.Configuration;

namespace Spirantis.Extensions.Configuration.Tests;

public sealed class ApplicationConfigurationTests
{
    private sealed class RecordingAgent : IConfigurationSourceAgent
    {
        public bool WasCalled { get; private set; }

        public IConfiguration? ReceivedEnvironment { get; private set; }

        public Action<IConfigurationBuilder>? Contribute { get; init; }

        public void Add(IConfigurationBuilder builder, IConfiguration environmentConfiguration)
        {
            WasCalled = true;
            ReceivedEnvironment = environmentConfiguration;
            Contribute?.Invoke(builder);
        }
    }

    private static IDictionary<string, string> NoSwitchMappings() =>
        new Dictionary<string, string>();

    [Fact]
    public void AddConfiguration_InvokesAgents_AndAppliesContributedValues()
    {
        var agent = new RecordingAgent
        {
            Contribute = builder =>
                builder.AddInMemoryCollection(new Dictionary<string, string?> { ["Foo"] = "Bar" }),
        };
        var configurationBuilder = new ConfigurationBuilder();

        configurationBuilder.AddConfiguration(
            NoSwitchMappings(),
            [],
            options => options.ConfigurationSourceAgents.Add(agent)
        );

        var configuration = configurationBuilder.Build();
        Assert.True(agent.WasCalled);
        Assert.Equal("Bar", configuration["Foo"]);
    }

    [Fact]
    public void AddConfiguration_AppliesCommandLineArguments_WithFinalPrecedence()
    {
        var agent = new RecordingAgent
        {
            Contribute = builder =>
                builder.AddInMemoryCollection(
                    new Dictionary<string, string?> { ["Key"] = "fromAgent" }
                ),
        };
        var configurationBuilder = new ConfigurationBuilder();

        configurationBuilder.AddConfiguration(
            NoSwitchMappings(),
            ["--Key=fromCommandLine"],
            options => options.ConfigurationSourceAgents.Add(agent)
        );

        var configuration = configurationBuilder.Build();
        Assert.Equal("fromCommandLine", configuration["Key"]);
    }

    [Fact]
    public void AddConfiguration_PassesEnvironmentConfigurationToAgent()
    {
        var agent = new RecordingAgent();
        var configurationBuilder = new ConfigurationBuilder();

        configurationBuilder.AddConfiguration(
            NoSwitchMappings(),
            ["--Sample=value"],
            options => options.ConfigurationSourceAgents.Add(agent)
        );

        Assert.NotNull(agent.ReceivedEnvironment);
        Assert.Equal("value", agent.ReceivedEnvironment!["Sample"]);
    }

    [Fact]
    public void AddConfiguration_PropagatesAgentException()
    {
        var agent = new RecordingAgent
        {
            Contribute = _ => throw new InvalidOperationException("boom"),
        };
        var configurationBuilder = new ConfigurationBuilder();

        Assert.Throws<InvalidOperationException>(() =>
            configurationBuilder.AddConfiguration(
                NoSwitchMappings(),
                [],
                options => options.ConfigurationSourceAgents.Add(agent)
            )
        );
    }

    [Fact]
    public void AddConfiguration_WithNullBuilder_Throws()
    {
        IConfigurationBuilder configurationBuilder = null!;

        Assert.Throws<ArgumentNullException>(() =>
            configurationBuilder.AddConfiguration(NoSwitchMappings(), [])
        );
    }

    [Fact]
    public void AddConfiguration_ReadsEnvironmentVariables()
    {
        const string Name = "SPIRANTIS_EXTENSIONS_CONFIG_TEST";
        Environment.SetEnvironmentVariable(Name, "env-value");

        try
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddConfiguration(NoSwitchMappings(), []);

            var configuration = configurationBuilder.Build();
            Assert.Equal("env-value", configuration[Name]);
        }
        finally
        {
            Environment.SetEnvironmentVariable(Name, null);
        }
    }
}
