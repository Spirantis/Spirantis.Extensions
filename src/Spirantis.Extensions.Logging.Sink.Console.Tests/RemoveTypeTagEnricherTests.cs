using Serilog.Events;
using Serilog.Parsing;
using Spirantis.Extensions.Logging.Sink.Console.Enrichers;

namespace Spirantis.Extensions.Logging.Sink.Console.Tests;

public sealed class RemoveTypeTagEnricherTests
{
    [Fact]
    public void Enrich_RemovesTypeTag_FromNestedStructuredProperties()
    {
        var inner = new StructureValue(
            [new LogEventProperty("X", new ScalarValue(1))],
            typeTag: "Nested"
        );
        var outer = new StructureValue([new LogEventProperty("Y", inner)], typeTag: "Outer");

        var logEvent = new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            exception: null,
            new MessageTemplateParser().Parse("test"),
            [new LogEventProperty("Obj", outer)]
        );

        new RemoveTypeTagEnricher().Enrich(logEvent, propertyFactory: null!);

        var enrichedOuter = Assert.IsType<StructureValue>(logEvent.Properties["Obj"]);
        Assert.Null(enrichedOuter.TypeTag);

        var enrichedInner = Assert.IsType<StructureValue>(
            Assert.Single(enrichedOuter.Properties).Value
        );
        Assert.Null(enrichedInner.TypeTag);
    }

    [Fact]
    public void Enrich_LeavesScalarPropertiesUnchanged()
    {
        var logEvent = new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            exception: null,
            new MessageTemplateParser().Parse("test"),
            [new LogEventProperty("Count", new ScalarValue(42))]
        );

        new RemoveTypeTagEnricher().Enrich(logEvent, propertyFactory: null!);

        var value = Assert.IsType<ScalarValue>(logEvent.Properties["Count"]);
        Assert.Equal(42, value.Value);
    }
}
