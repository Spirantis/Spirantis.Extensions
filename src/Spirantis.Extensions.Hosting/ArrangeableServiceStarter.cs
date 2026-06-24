using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Spirantis.Extensions.Hosting;

/// <summary>
/// The single <see cref="IHostedService"/> that drives all registered
/// <see cref="IArrangeableHostedService"/> instances: it starts them in
/// <see cref="HostingArrangement"/> order and stops them in the reverse order.
/// </summary>
public sealed class ArrangeableServiceStarter : IHostedService
{
    private readonly IReadOnlyList<IArrangeableHostedService> orderedServices;
    private readonly ILogger<ArrangeableServiceStarter> logger;

    /// <summary>Creates a starter that orders and drives the given arrangeable services.</summary>
    /// <param name="services">The arrangeable hosted services to order and run.</param>
    /// <param name="logger">Logger used to report failures encountered while stopping services.</param>
    public ArrangeableServiceStarter(
        IEnumerable<IArrangeableHostedService> services,
        ILogger<ArrangeableServiceStarter> logger
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        this.logger = logger;

        // OrderBy is stable, so services within a tier keep their registration order.
        orderedServices = [.. services.OrderBy(service => service.Arrangement)];
    }

    /// <summary>
    /// Starts each service in arrangement order. Fails fast: the first start exception
    /// propagates so the host does not start in a partially-initialised state.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var service in orderedServices)
        {
            await service.StartAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Stops each service in reverse arrangement order, continuing past failures so every
    /// service gets a stop signal; any exceptions are aggregated and rethrown.
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        List<Exception>? exceptions = null;

        for (int index = orderedServices.Count - 1; index >= 0; index--)
        {
            try
            {
                await orderedServices[index].StopAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Error stopping arrangeable service {ServiceType}",
                    orderedServices[index].GetType().FullName
                );
                (exceptions ??= []).Add(exception);
            }
        }

        if (exceptions is not null)
        {
            throw new AggregateException(exceptions);
        }
    }
}
