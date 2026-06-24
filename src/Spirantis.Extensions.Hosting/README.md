# Spirantis.Extensions.Hosting

Priority-ordered hosted services for .NET. A hosted service declares a **tier**
(`Superior` / `Normal` / `Inferior`); a single starter then starts them in that order and
stops them in the reverse order — filling the gap that the framework starts hosted
services in registration order with no notion of phases. Container-agnostic.

## Installation

```bash
dotnet add package Spirantis.Extensions.Hosting
```

## Requirements

- .NET 10.0 or later

## Usage

Derive from `ArrangeableBackgroundService` (a framework `BackgroundService` plus a tier) or
`ArrangeableHostedService` (explicit start/stop), then register:

```csharp
using Spirantis.Extensions.Hosting;

public sealed class CacheWarmer() : ArrangeableBackgroundService(HostingArrangement.Superior)
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) { /* ... */ }
}

services.AddArrangeableHostedService<CacheWarmer>();
```

`AddArrangeableHostedService<T>` registers the service and ensures the single
`ArrangeableServiceStarter` is registered. The starter **fails fast** if a service throws
during startup, and stops services in reverse (LIFO) order during shutdown.

If you register the services through another container (for example Autofac — see
[`Spirantis.Extensions.Hosting.Autofac`](https://www.nuget.org/packages/Spirantis.Extensions.Hosting.Autofac)),
register just the starter with `services.AddArrangeableServiceStarter()`.

## License

[MIT](LICENSE)
