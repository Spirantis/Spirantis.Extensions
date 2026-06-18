# Spirantis.Extensions.Configuration

Composable application configuration for .NET. It layers configuration from
environment variables and command-line arguments, then runs a set of pluggable
**configuration source agents** that can contribute additional sources (JSON files,
AWS Parameter Store, and so on), before applying command-line arguments once more so
they always take final precedence.

## Installation

```bash
dotnet add package Spirantis.Extensions.Configuration
```

## Requirements

- .NET 10.0 or later

## Usage

Call `AddConfiguration` on an `IConfigurationBuilder`, registering any source agents
you need:

```csharp
using Microsoft.Extensions.Configuration;
using Spirantis.Extensions.Configuration;

var switchMappings = new Dictionary<string, string>();

var configuration = new ConfigurationBuilder()
    .AddConfiguration(
        switchMappings,
        args,
        options =>
        {
            options.BaseDirectory = AppContext.BaseDirectory;
            options.ConfigurationSourceAgents.Add(new ServiceJsonFileSourceAgent());
            options.ConfigurationSourceAgents.Add(new AWSParameterStoreSourceAgent());
        })
    .Build();
```

The precedence, from lowest to highest, is:

1. Environment variables
2. Command-line arguments
3. Each registered `IConfigurationSourceAgent`, in registration order
4. Command-line arguments (re-applied so they always win)

### Writing a source agent

Implement `IConfigurationSourceAgent` to plug in your own source. The agent receives
the builder plus an already-built *environment configuration* (environment variables
and command-line arguments) so it can read its own switches:

```csharp
public sealed class MySourceAgent : IConfigurationSourceAgent
{
    public void Add(IConfigurationBuilder builder, IConfiguration environmentConfiguration)
    {
        if (environmentConfiguration.GetValue("MY_SOURCE_ENABLE", false))
        {
            builder.AddInMemoryCollection(/* ... */);
        }
    }
}
```

### Environment helpers

`EnvironmentSettings` exposes the well-known `Environment` section:

```csharp
string name = configuration.GetEnvironmentName();        // "Environment:Name", or "undefined"
string? prefix = configuration.GetEnvironmentPrefix();   // the name when "Environment:UseEnvPrefix" is true
```

## Related packages

- [`Spirantis.Extensions.Configuration.Source.Json`](https://www.nuget.org/packages/Spirantis.Extensions.Configuration.Source.Json) — machine-wide and service-local JSON file sources.
- [`Spirantis.Extensions.Configuration.Source.AWSParameterStore`](https://www.nuget.org/packages/Spirantis.Extensions.Configuration.Source.AWSParameterStore) — AWS Systems Manager Parameter Store source.

## License

[MIT](LICENSE)
