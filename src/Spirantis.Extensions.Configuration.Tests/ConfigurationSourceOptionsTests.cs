namespace Spirantis.Extensions.Configuration.Tests;

public sealed class ConfigurationSourceOptionsTests
{
    [Fact]
    public void Defaults_BaseDirectory_IsApplicationBaseDirectory()
    {
        var options = new ConfigurationSourceOptions();

        Assert.Equal(AppContext.BaseDirectory, options.BaseDirectory);
    }

    [Fact]
    public void Defaults_ConfigurationSourceAgents_IsEmpty()
    {
        var options = new ConfigurationSourceOptions();

        Assert.Empty(options.ConfigurationSourceAgents);
    }
}
