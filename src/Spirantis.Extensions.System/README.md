# Spirantis.Extensions.System

Lightweight, dependency-free extension methods for common `System` types — date/time conversions, span-aware string trimming, and reflection-based type discovery.

All extensions live in the `System` namespace, so they're available without an extra `using` once the package is referenced.

## Installation

```bash
dotnet add package Spirantis.Extensions.System
```

## Requirements

- .NET 10.0 or later

## Features

### `DateTime` extensions

```csharp
DateTime now = DateTime.UtcNow;

// Milliseconds since the Unix epoch (floored)
double ms = now.ToMillisecondTimestamp();

// Minutes elapsed since midnight (0–1439)
int minutes = now.ToMinutesSinceMidnight();

// Seconds elapsed since midnight (0–86399)
int seconds = now.ToSecondsSinceMidnight();
```

### `string` / `ReadOnlySpan<char>` extensions

Trim a whole substring (not just individual characters) from either end, with a configurable `StringComparison`:

```csharp
"file.txt.txt".TrimEnd(".txt", StringComparison.Ordinal);
// => "file"

"   prefix-prefix-value".AsSpan()
    .TrimStart("prefix-", StringComparison.Ordinal);
// => "value"  (span overload, allocation-free)
```

Overloads are provided for both `string` (returns `string`) and `ReadOnlySpan<char>` (returns `ReadOnlySpan<char>`, no allocation).

### `IEnumerable<Type>` extensions

Find every type that derives from a given base type or open generic, at any depth:

```csharp
Type[] allTypes = typeof(MyClass).Assembly.GetTypes();

// Non-generic base type (transitive — all descendants)
IEnumerable<Type> shapes = allTypes.Derives(typeof(Shape));

// Open generic base type (e.g. all types deriving from Repository<T>)
IEnumerable<Type> repos = allTypes.Derives(typeof(Repository<>));
```

## License

[MIT](LICENSE)
