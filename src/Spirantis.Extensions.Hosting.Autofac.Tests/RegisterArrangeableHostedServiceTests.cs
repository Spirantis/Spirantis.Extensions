using global::Autofac;
using Spirantis.Extensions.Hosting;

namespace Spirantis.Extensions.Hosting.Autofac.Tests;

public sealed class RegisterArrangeableHostedServiceTests
{
    private sealed class SampleService() : ArrangeableHostedService(HostingArrangement.Superior)
    {
        public override Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public override Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    [Fact]
    public void RegisterArrangeableHostedService_RegistersAsArrangeableHostedService()
    {
        var builder = new ContainerBuilder();
        builder.RegisterArrangeableHostedService<SampleService>();

        using var container = builder.Build();

        var service = Assert.Single(container.Resolve<IEnumerable<IArrangeableHostedService>>());
        Assert.IsType<SampleService>(service);
    }

    [Fact]
    public void RegisterArrangeableHostedService_IsSingleton()
    {
        var builder = new ContainerBuilder();
        builder.RegisterArrangeableHostedService<SampleService>();

        using var container = builder.Build();

        var first = container.Resolve<IEnumerable<IArrangeableHostedService>>().Single();
        var second = container.Resolve<IEnumerable<IArrangeableHostedService>>().Single();
        Assert.Same(first, second);
    }

    [Fact]
    public void RegisterArrangeableHostedService_ReturnsChainableBuilder()
    {
        var builder = new ContainerBuilder();

        var registration = builder.RegisterArrangeableHostedService<SampleService>();

        Assert.NotNull(registration);
    }
}
