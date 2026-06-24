# Spirantis.Extensions.DependencyInjection.Autofac

Autofac helpers for binding **named** `IOptions<T>` into the container and for wiring a
**specific named dependency into an individual constructor parameter** at registration
time — registration-time wiring that MS DI's keyed services don't express as flexibly.

## Installation

```bash
dotnet add package Spirantis.Extensions.DependencyInjection.Autofac
```

## Requirements

- .NET 10.0 or later

## Named options

```csharp
using Autofac;

builder.RegisterNamedConfiguration<KafkaOptions>(configuration.GetSection("Producer"), "producer");
builder.RegisterNamedConfiguration(new KafkaOptions { ... }, "consumer");

// Default (non-named) options bound to configuration, with reload support:
builder.RegisterConfiguration<KafkaOptions>(configuration);
```

## Per-parameter named resolution

`ParameterResolving` builds Autofac `ResolvedParameter`s that target a single constructor
parameter and resolve it from a named registration:

```csharp
using Spirantis.Extensions.DependencyInjection.Autofac;

builder.RegisterType<EventPublisher>()
    .WithParameters(
        ParameterResolving.CreateResolvedParameter<IMessageProducer>("producer"),
        ParameterResolving.CreateResolvedOptionsParameter<KafkaOptions>("producer"));
```

- `CreateResolvedParameter<T>("name")` — resolve a `T` parameter from the named registration.
- `CreateResolvedParameter<T>(value)` — supply a fixed instance for a `T` parameter.
- `CreateResolvedOptionsParameter<TOptions>("name")` — resolve an `IOptions<TOptions>` parameter from a named registration.
- `WithParameters(params Parameter[])` — a `params` convenience over Autofac's `WithParameters(IEnumerable<Parameter>)`.

## License

[MIT](LICENSE)
