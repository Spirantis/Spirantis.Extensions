using Microsoft.Extensions.DependencyInjection;

namespace Spirantis.Extensions.Hosting;

/// <summary>
/// Registration helpers for arrangeable hosted services on an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <typeparamref name="TService"/> as an <see cref="IArrangeableHostedService"/> and
    /// ensures the <see cref="ArrangeableServiceStarter"/> is registered to drive it.
    /// </summary>
    public static IServiceCollection AddArrangeableHostedService<TService>(
        this IServiceCollection services
    )
        where TService : class, IArrangeableHostedService
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IArrangeableHostedService, TService>();
        return services.AddArrangeableServiceStarter();
    }

    /// <summary>
    /// Registers the <see cref="ArrangeableServiceStarter"/> hosted service. Safe to call
    /// repeatedly (the starter is registered once). Use this when the arrangeable services
    /// themselves are registered through another container (for example Autofac).
    /// </summary>
    public static IServiceCollection AddArrangeableServiceStarter(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // AddHostedService uses TryAddEnumerable, so repeated calls register a single starter.
        services.AddHostedService<ArrangeableServiceStarter>();
        return services;
    }
}
