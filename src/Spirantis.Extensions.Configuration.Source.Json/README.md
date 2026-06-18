# Spirantis.Extensions.Configuration.Source.Json

JSON file configuration source agents for
[`Spirantis.Extensions.Configuration`](https://www.nuget.org/packages/Spirantis.Extensions.Configuration).
Provides two `IConfigurationSourceAgent` implementations that add a `config.json`
file to the configuration when their enabling environment key is set.

## Installation

```bash
dotnet add package Spirantis.Extensions.Configuration.Source.Json
```

## Requirements

- .NET 10.0 or later

## Agents

### `ServiceJsonFileSourceAgent`

Adds a service-local JSON file resolved relative to the builder's base path.
Enabled by the `SPIRANTIS_CONFIG_JSON_SERVICE_ENABLE` environment key.

```csharp
options.ConfigurationSourceAgents.Add(new ServiceJsonFileSourceAgent
{
    FileName = "config",     // loads "config.json"
    IsOptional = true,
    ReloadOnChange = true,
});
```

### `GlobalJsonFileSourceAgent`

Adds a machine-wide JSON file, searching a configurable list of candidate
directories and loading the file from the first one that exists. Enabled by the
`SPIRANTIS_CONFIG_JSON_GLOBAL_ENABLE` environment key.

```csharp
var agent = new GlobalJsonFileSourceAgent();

// Candidate directories are searched in order; the first that exists wins.
agent.SetRelativeConfigurationPath("/etc/myapp");   // replace the defaults
agent.AddRelativeConfigurationPath("../Configuration"); // or append one

options.ConfigurationSourceAgents.Add(agent);
```

The default search paths are origin-neutral (`/etc/spirantis/`, `/opt/spirantis/etc`,
and a chain of `../Configuration` relatives) and fully overridable. When the agent is
enabled but no candidate directory exists, `Add` throws `DirectoryNotFoundException`.

## License

[MIT](LICENSE)
