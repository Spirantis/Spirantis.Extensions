# Spirantis.Extensions.Hosting.Autofac

Autofac registration glue for
[`Spirantis.Extensions.Hosting`](https://www.nuget.org/packages/Spirantis.Extensions.Hosting):
register arrangeable hosted services into an Autofac `ContainerBuilder`.

## Installation

```bash
dotnet add package Spirantis.Extensions.Hosting.Autofac
```

## Usage

```csharp
using Autofac;

builder.RegisterArrangeableHostedService<CacheWarmer>();

// The call returns the registration, so it chains with the named-parameter helpers
// from Spirantis.Extensions.DependencyInjection.Autofac:
builder.RegisterArrangeableHostedService<EventListener>()
    .WithParameters(ParameterResolving.CreateResolvedParameter<IMessageConsumer>("consumer"));
```

Register the starter itself once via `services.AddArrangeableServiceStarter()` on the
`IServiceCollection` (it resolves the Autofac-registered services through the shared
container when using `AutofacServiceProviderFactory`).

## License

[MIT](LICENSE)
