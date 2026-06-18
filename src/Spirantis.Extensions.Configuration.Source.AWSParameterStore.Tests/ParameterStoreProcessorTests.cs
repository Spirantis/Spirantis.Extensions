using Amazon.SimpleSystemsManagement.Model;

namespace Spirantis.Extensions.Configuration.Source.AWSParameterStore.Tests;

public sealed class ParameterStoreProcessorTests
{
    [Fact]
    public void ProcessParameters_FlattensNestedJsonValue()
    {
        var parameters = new[]
        {
            new Parameter
            {
                Name = "/app/settings",
                Value = """{ "Db": { "Host": "localhost", "Port": 5432 } }""",
            },
        };

        var result = ParameterStoreProcessor.ProcessParameters(parameters, "/app");

        Assert.Equal("localhost", result["settings:Db:Host"]);
        Assert.Equal("5432", result["settings:Db:Port"]);
    }

    [Fact]
    public void ProcessParameters_FlattensJsonArray_UsingIndices()
    {
        var parameters = new[]
        {
            new Parameter { Name = "/app/list", Value = """{ "Items": ["a", "b"] }""" },
        };

        var result = ParameterStoreProcessor.ProcessParameters(parameters, "/app");

        Assert.Equal("a", result["list:Items:0"]);
        Assert.Equal("b", result["list:Items:1"]);
    }

    [Fact]
    public void ProcessParameters_FallsBackToRawValue_WhenNotJson()
    {
        var parameters = new[]
        {
            new Parameter { Name = "/app/plain", Value = "just-text" },
        };

        var result = ParameterStoreProcessor.ProcessParameters(parameters, "/app");

        Assert.Equal("just-text", result["plain"]);
    }

    [Fact]
    public void ProcessParameters_ReplacesSlashesWithDelimiter_WhenNameOutsideBasePath()
    {
        var parameters = new[]
        {
            new Parameter { Name = "other/key", Value = "value" },
        };

        var result = ParameterStoreProcessor.ProcessParameters(parameters, "/app");

        Assert.Equal("value", result["other:key"]);
    }

    [Fact]
    public void ProcessParameters_PreservesNullForJsonNull()
    {
        var parameters = new[]
        {
            new Parameter { Name = "/app/value", Value = """{ "Missing": null }""" },
        };

        var result = ParameterStoreProcessor.ProcessParameters(parameters, "/app");

        Assert.True(result.ContainsKey("value:Missing"));
        Assert.Null(result["value:Missing"]);
    }

    [Fact]
    public void AddPrefix_PrependsPrefixToEveryKey()
    {
        var input = new Dictionary<string, string?> { ["A"] = "1", ["B:C"] = "2" };

        var result = ParameterStoreProcessor.AddPrefix(input, "prefix");

        Assert.Equal("1", result["prefix:A"]);
        Assert.Equal("2", result["prefix:B:C"]);
    }

    [Fact]
    public void AddPrefix_WithoutPrefix_ReturnsSameInstance()
    {
        var input = new Dictionary<string, string?> { ["A"] = "1" };

        Assert.Same(input, ParameterStoreProcessor.AddPrefix(input, null));
        Assert.Same(input, ParameterStoreProcessor.AddPrefix(input, string.Empty));
    }

    [Fact]
    public void ProcessParameters_WithNullArguments_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ParameterStoreProcessor.ProcessParameters(null!, "/app")
        );
        Assert.Throws<ArgumentNullException>(() =>
            ParameterStoreProcessor.ProcessParameters([], null!)
        );
    }

    [Fact]
    public void AddPrefix_WithNullInput_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ParameterStoreProcessor.AddPrefix(null!, "p"));
    }
}
