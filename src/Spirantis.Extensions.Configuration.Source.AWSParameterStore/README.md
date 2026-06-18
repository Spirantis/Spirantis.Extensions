# Spirantis.Extensions.Configuration.Source.AWSParameterStore

An AWS Systems Manager Parameter Store configuration source for
[`Spirantis.Extensions.Configuration`](https://www.nuget.org/packages/Spirantis.Extensions.Configuration).
It loads parameters recursively under one or more base paths, flattens JSON-valued
parameters into configuration keys, and optionally reloads them on an interval.

## Installation

```bash
dotnet add package Spirantis.Extensions.Configuration.Source.AWSParameterStore
```

## Requirements

- .NET 10.0 or later

## Usage

Register `AWSParameterStoreSourceAgent` as a configuration source agent. It activates
when its enabling environment key is set and reads its AWS settings from the
environment configuration. All environment keys are configurable; their defaults use
a neutral `SPIRANTIS_CONFIG_` prefix.

```csharp
options.ConfigurationSourceAgents.Add(new AWSParameterStoreSourceAgent
{
    Prefix = "Aws",                       // optional prefix for produced keys
    ReloadAfter = TimeSpan.FromMinutes(5), // optional periodic reload
    IsOptional = true,
});
```

### Environment keys

| Default key | Purpose |
| ----------- | ------- |
| `SPIRANTIS_CONFIG_AWS_PS_ENABLE` | Enables the agent. |
| `SPIRANTIS_CONFIG_AWS_PS_PATH` | Base path to load recursively (any key *containing* this name is treated as a base path, so multiple paths are supported). |
| `SPIRANTIS_CONFIG_AWS_PS_REGION` | AWS region system name. |
| `SPIRANTIS_CONFIG_AWS_PS_ACCKEY` / `SPIRANTIS_CONFIG_AWS_PS_SECKEY` | Optional explicit credentials. |
| `SPIRANTIS_CONFIG_AWS_ENDPOINT` | Optional custom service endpoint. |
| `SPIRANTIS_CONFIG_AWS_LOGGING` | When set, enables verbose AWS SDK logging. |

### JSON flattening

Parameter values that are valid JSON are flattened into hierarchical configuration
keys (objects by property name, arrays by index), joined with the standard `:`
delimiter. Values that are not JSON are stored verbatim. For example a parameter
`/app/settings` with value `{ "Db": { "Host": "localhost" } }` loaded under base path
`/app` produces the key `settings:Db:Host`.

JSON parsing uses `System.Text.Json`.

## Advanced

`ParameterStoreConfigurationSource`, `ParameterStoreConfigurationProvider`, and
`IParameterStoreProcessor` are public so you can build the source directly or supply
a custom processor (for example, in tests).

## License

[MIT](LICENSE)
