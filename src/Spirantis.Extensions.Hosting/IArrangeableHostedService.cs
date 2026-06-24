namespace Spirantis.Extensions.Hosting;

/// <summary>
/// A hosted service that participates in ordered startup/shutdown via its
/// <see cref="Arrangement"/> tier. Started and stopped by <see cref="ArrangeableServiceStarter"/>
/// rather than directly by the host.
/// </summary>
public interface IArrangeableHostedService
{
    /// <summary>The start/stop priority tier.</summary>
    HostingArrangement Arrangement { get; }

    /// <summary>Starts the service.</summary>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>Stops the service.</summary>
    Task StopAsync(CancellationToken cancellationToken);
}
