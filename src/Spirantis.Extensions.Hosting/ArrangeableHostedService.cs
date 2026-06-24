namespace Spirantis.Extensions.Hosting;

/// <summary>
/// Base class for an <see cref="IArrangeableHostedService"/> with explicit start/stop logic.
/// Pass a <see cref="HostingArrangement"/> to place it in a non-default tier.
/// </summary>
public abstract class ArrangeableHostedService(
    HostingArrangement arrangement = HostingArrangement.Normal
) : IArrangeableHostedService
{
    /// <inheritdoc />
    public HostingArrangement Arrangement { get; } = arrangement;

    /// <inheritdoc />
    public abstract Task StartAsync(CancellationToken cancellationToken);

    /// <inheritdoc />
    public abstract Task StopAsync(CancellationToken cancellationToken);
}
