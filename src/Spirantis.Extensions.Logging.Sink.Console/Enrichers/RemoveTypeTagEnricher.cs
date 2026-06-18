using Serilog.Core;
using Serilog.Events;

namespace Spirantis.Extensions.Logging.Sink.Console.Enrichers;

/// <summary>
/// A Serilog enricher that strips the CLR type tag from structured (object) properties,
/// producing cleaner JSON output that omits the synthetic <c>$type</c> field.
/// </summary>
public sealed class RemoveTypeTagEnricher : ILogEventEnricher
{
    /// <inheritdoc />
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        ArgumentNullException.ThrowIfNull(logEvent);

        foreach (var property in logEvent.Properties)
        {
            if (property.Value is StructureValue structureValue)
            {
                logEvent.AddOrUpdateProperty(
                    new LogEventProperty(property.Key, RemoveTypeTag(structureValue))
                );
            }
        }
    }

    private static StructureValue RemoveTypeTag(StructureValue structureValue) =>
        new(RemoveTypeTag(structureValue.Properties), typeTag: null);

    private static IEnumerable<LogEventProperty> RemoveTypeTag(
        IEnumerable<LogEventProperty> properties
    )
    {
        foreach (var property in properties)
        {
            yield return property.Value is StructureValue structureValue
                ? new LogEventProperty(property.Name, RemoveTypeTag(structureValue))
                : property;
        }
    }
}
