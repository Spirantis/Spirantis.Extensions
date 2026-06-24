using global::Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Spirantis.Extensions.DependencyInjection.Autofac.Tests;

public sealed class AutofacDependencyInjectionTests
{
    private interface IGreeter
    {
        string Name { get; }
    }

    private sealed class Greeter(string name) : IGreeter
    {
        public string Name => name;
    }

    private sealed class Consumer(IGreeter greeter)
    {
        public IGreeter Greeter => greeter;
    }

    private sealed class WidgetOptions
    {
        public int Size { get; set; }
    }

    private sealed class OptionsConsumer(IOptions<WidgetOptions> options)
    {
        public int Size => options.Value.Size;
    }

    [Fact]
    public void CreateResolvedParameter_WiresNamedDependency_IntoConstructorParameter()
    {
        var builder = new ContainerBuilder();
        builder.RegisterInstance(new Greeter("alice")).Named<IGreeter>("a");
        builder.RegisterInstance(new Greeter("bob")).Named<IGreeter>("b");
        builder
            .RegisterType<Consumer>()
            .WithParameters(ParameterResolving.CreateResolvedParameter<IGreeter>("b"));

        using var container = builder.Build();

        Assert.Equal("bob", container.Resolve<Consumer>().Greeter.Name);
    }

    [Fact]
    public void CreateResolvedParameter_WithFixedValue_SuppliesValue()
    {
        var builder = new ContainerBuilder();
        builder
            .RegisterType<Consumer>()
            .WithParameters(
                ParameterResolving.CreateResolvedParameter<IGreeter>(new Greeter("fixed"))
            );

        using var container = builder.Build();

        Assert.Equal("fixed", container.Resolve<Consumer>().Greeter.Name);
    }

    [Fact]
    public void RegisterNamedConfiguration_FromInstance_RegistersNamedOptions()
    {
        var builder = new ContainerBuilder();
        builder.RegisterNamedConfiguration(new WidgetOptions { Size = 42 }, "x");

        using var container = builder.Build();

        Assert.Equal(42, container.ResolveNamed<IOptions<WidgetOptions>>("x").Value.Size);
    }

    [Fact]
    public void RegisterNamedConfiguration_FromConfiguration_BindsAndRegistersNamedOptions()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Size"] = "99" })
            .Build();
        var builder = new ContainerBuilder();
        builder.RegisterNamedConfiguration<WidgetOptions>(configuration, "x");

        using var container = builder.Build();

        Assert.Equal(99, container.ResolveNamed<IOptions<WidgetOptions>>("x").Value.Size);
    }

    [Fact]
    public void CreateResolvedOptionsParameter_WiresNamedOptions_IntoConstructorParameter()
    {
        var builder = new ContainerBuilder();
        builder.RegisterNamedConfiguration(new WidgetOptions { Size = 7 }, "x");
        builder
            .RegisterType<OptionsConsumer>()
            .WithParameters(ParameterResolving.CreateResolvedOptionsParameter<WidgetOptions>("x"));

        using var container = builder.Build();

        Assert.Equal(7, container.Resolve<OptionsConsumer>().Size);
    }
}
