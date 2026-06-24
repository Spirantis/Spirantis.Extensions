using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Spirantis.Extensions.Hosting.Tests;

public sealed class ArrangeableServiceStarterTests
{
    private sealed class RecordingService(
        string id,
        HostingArrangement arrangement,
        List<string> log
    ) : ArrangeableHostedService(arrangement)
    {
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            log.Add($"start:{id}");
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            log.Add($"stop:{id}");
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingService(HostingArrangement arrangement)
        : ArrangeableHostedService(arrangement)
    {
        public override Task StartAsync(CancellationToken cancellationToken) =>
            throw new InvalidOperationException("boom");

        public override Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class SampleA() : ArrangeableHostedService(HostingArrangement.Normal)
    {
        public override Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public override Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class SampleB() : ArrangeableHostedService(HostingArrangement.Superior)
    {
        public override Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public override Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private static ArrangeableServiceStarter Starter(params IArrangeableHostedService[] services) =>
        new(services, NullLogger<ArrangeableServiceStarter>.Instance);

    [Fact]
    public async Task StartAsync_StartsServices_InArrangementOrder()
    {
        var log = new List<string>();
        var starter = Starter(
            new RecordingService("normal", HostingArrangement.Normal, log),
            new RecordingService("inferior", HostingArrangement.Inferior, log),
            new RecordingService("superior", HostingArrangement.Superior, log)
        );

        await starter.StartAsync(CancellationToken.None);

        Assert.Equal(["start:superior", "start:normal", "start:inferior"], log);
    }

    [Fact]
    public async Task StopAsync_StopsServices_InReverseOrder()
    {
        var log = new List<string>();
        var starter = Starter(
            new RecordingService("superior", HostingArrangement.Superior, log),
            new RecordingService("normal", HostingArrangement.Normal, log),
            new RecordingService("inferior", HostingArrangement.Inferior, log)
        );
        await starter.StartAsync(CancellationToken.None);
        log.Clear();

        await starter.StopAsync(CancellationToken.None);

        Assert.Equal(["stop:inferior", "stop:normal", "stop:superior"], log);
    }

    [Fact]
    public async Task StartAsync_FailsFast_AndDoesNotStartLaterServices()
    {
        var log = new List<string>();
        var starter = Starter(
            new ThrowingService(HostingArrangement.Superior),
            new RecordingService("after", HostingArrangement.Normal, log)
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            starter.StartAsync(CancellationToken.None)
        );
        Assert.DoesNotContain("start:after", log);
    }

    [Fact]
    public void AddArrangeableHostedService_RegistersServices_AndASingleStarter()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddArrangeableHostedService<SampleA>();
        services.AddArrangeableHostedService<SampleB>();

        using var provider = services.BuildServiceProvider();

        Assert.Equal(2, provider.GetServices<IArrangeableHostedService>().Count());
        Assert.Single(provider.GetServices<IHostedService>().OfType<ArrangeableServiceStarter>());
    }
}
