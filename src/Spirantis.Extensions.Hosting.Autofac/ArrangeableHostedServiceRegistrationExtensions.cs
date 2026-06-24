using Autofac.Builder;
using Spirantis.Extensions.Hosting;

namespace Autofac;

/// <summary>
/// Autofac registration helpers for arrangeable hosted services.
/// </summary>
public static class ArrangeableHostedServiceRegistrationExtensions
{
    /// <summary>
    /// Registers <typeparamref name="TService"/> as a singleton
    /// <see cref="IArrangeableHostedService"/>. The returned builder can be chained (for example
    /// with <c>WithParameters(...)</c>). Register the
    /// <see cref="ArrangeableServiceStarter"/> separately via
    /// <c>services.AddArrangeableServiceStarter()</c>.
    /// </summary>
    public static IRegistrationBuilder<
        TService,
        ConcreteReflectionActivatorData,
        SingleRegistrationStyle
    > RegisterArrangeableHostedService<TService>(this ContainerBuilder builder)
        where TService : class, IArrangeableHostedService
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.RegisterType<TService>().As<IArrangeableHostedService>().SingleInstance();
    }
}
