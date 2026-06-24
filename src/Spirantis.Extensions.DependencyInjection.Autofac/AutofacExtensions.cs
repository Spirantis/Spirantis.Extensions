using Autofac.Builder;
using Autofac.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Autofac;

/// <summary>
/// Autofac registration helpers for binding <see cref="IOptions{TOptions}"/> (optionally
/// named) into the container and for fluent parameter registration.
/// </summary>
public static class AutofacExtensions
{
    /// <summary>
    /// Registers <typeparamref name="TOptions"/> as the default <see cref="IOptions{TOptions}"/>,
    /// bound to <paramref name="configuration"/> with reload support.
    /// </summary>
    public static void RegisterConfiguration<TOptions>(
        this ContainerBuilder builder,
        IConfiguration configuration
    )
        where TOptions : class
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configuration);

        builder
            .RegisterInstance(
                new ConfigurationChangeTokenSource<TOptions>(string.Empty, configuration)
            )
            .As<IOptionsChangeTokenSource<TOptions>>()
            .SingleInstance();

        builder
            .RegisterInstance(
                new NamedConfigureFromConfigurationOptions<TOptions>(string.Empty, configuration)
            )
            .As<IConfigureOptions<TOptions>>()
            .SingleInstance();
    }

    /// <summary>
    /// Registers a named <see cref="IOptions{TOptions}"/> bound from <paramref name="configuration"/>.
    /// </summary>
    public static void RegisterNamedConfiguration<TOptions>(
        this ContainerBuilder builder,
        IConfiguration configuration,
        string name
    )
        where TOptions : class, new()
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .RegisterInstance(GetOptions<TOptions>(configuration))
            .Named<IOptions<TOptions>>(name)
            .SingleInstance();
    }

    /// <summary>Registers a named <see cref="IOptions{TOptions}"/> from an existing instance.</summary>
    public static void RegisterNamedConfiguration<TOptions>(
        this ContainerBuilder builder,
        TOptions options,
        string name
    )
        where TOptions : class, new()
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .RegisterInstance(GetOptions(options))
            .Named<IOptions<TOptions>>(name)
            .SingleInstance();
    }

    /// <summary>Binds <paramref name="configuration"/> to a new <see cref="IOptions{TOptions}"/>.</summary>
    public static IOptions<TOptions> GetOptions<TOptions>(IConfiguration configuration)
        where TOptions : class, new()
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return GetOptions(configuration.Get<TOptions>() ?? new TOptions());
    }

    /// <summary>Wraps an existing instance as <see cref="IOptions{TOptions}"/>.</summary>
    public static IOptions<TOptions> GetOptions<TOptions>(TOptions options)
        where TOptions : class, new() => Options.Create(options);

    /// <summary>
    /// A <see langword="params"/> convenience over Autofac's
    /// <c>WithParameters(IEnumerable&lt;Parameter&gt;)</c>.
    /// </summary>
    public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> WithParameters<
        TLimit,
        TReflectionActivatorData,
        TStyle
    >(
        this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
        params Parameter[] parameters
    )
        where TReflectionActivatorData : ReflectionActivatorData
    {
        ArgumentNullException.ThrowIfNull(registration);
        ArgumentNullException.ThrowIfNull(parameters);

        // AsEnumerable() binds to Autofac's IEnumerable<Parameter> overload (not this one).
        return registration.WithParameters(parameters.AsEnumerable());
    }
}
