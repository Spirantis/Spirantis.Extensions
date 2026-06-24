using Microsoft.Extensions.Hosting;

namespace Spirantis.Extensions.Hosting;

/// <summary>
/// Base class for a long-running <see cref="IArrangeableHostedService"/>. Derives from the
/// framework <see cref="BackgroundService"/> (override <see cref="BackgroundService.ExecuteAsync"/>)
/// and adds an ordering <see cref="HostingArrangement"/>.
/// </summary>
public abstract class ArrangeableBackgroundService(
    HostingArrangement arrangement = HostingArrangement.Normal
) : BackgroundService, IArrangeableHostedService
{
    /// <inheritdoc />
    public HostingArrangement Arrangement { get; } = arrangement;
}
