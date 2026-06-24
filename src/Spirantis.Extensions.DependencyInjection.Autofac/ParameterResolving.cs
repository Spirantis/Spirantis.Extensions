using global::Autofac;
using global::Autofac.Core;
using Microsoft.Extensions.Options;

namespace Spirantis.Extensions.DependencyInjection.Autofac;

/// <summary>
/// Factory methods for Autofac <see cref="ResolvedParameter"/>s that wire a specific,
/// named dependency into a single constructor parameter at registration time.
/// </summary>
public static class ParameterResolving
{
    /// <summary>
    /// Resolves an <see cref="IOptions{TOptions}"/> constructor parameter from the named
    /// registration <paramref name="instanceName"/>.
    /// </summary>
    public static ResolvedParameter CreateResolvedOptionsParameter<TOptions>(string instanceName)
        where TOptions : class, new() =>
        new(
            (parameterInfo, _) => parameterInfo.ParameterType == typeof(IOptions<TOptions>),
            (_, context) => context.ResolveNamed<IOptions<TOptions>>(instanceName)
        );

    /// <summary>
    /// Resolves a <typeparamref name="TParameter"/> constructor parameter from the named
    /// registration <paramref name="instanceName"/>.
    /// </summary>
    public static ResolvedParameter CreateResolvedParameter<TParameter>(string instanceName)
        where TParameter : notnull =>
        new(
            (parameterInfo, _) => parameterInfo.ParameterType == typeof(TParameter),
            (_, context) => context.ResolveNamed<TParameter>(instanceName)
        );

    /// <summary>
    /// Supplies a fixed <paramref name="value"/> for a <typeparamref name="TParameter"/>
    /// constructor parameter.
    /// </summary>
    public static ResolvedParameter CreateResolvedParameter<TParameter>(TParameter value)
        where TParameter : notnull =>
        new(
            (parameterInfo, _) => parameterInfo.ParameterType == typeof(TParameter),
            (_, _) => value
        );
}
