using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Spirantis.Extensions.Configuration.Source.AWSParameterStore;

/// <summary>
/// Flattens a JSON document into configuration key/value pairs, joining nested
/// property and array indices with <see cref="ConfigurationPath.KeyDelimiter"/> and
/// rooting every key under a supplied base path.
/// </summary>
internal sealed class JsonConfigurationParser
{
    private readonly SortedDictionary<string, string?> data = new(StringComparer.OrdinalIgnoreCase);
    private readonly Stack<string> context = new();
    private string currentPath;

    private JsonConfigurationParser(string basePath)
    {
        currentPath = basePath;
        context.Push(basePath);
    }

    public static IDictionary<string, string?> Parse(string input, string basePath)
    {
        var parser = new JsonConfigurationParser(basePath);

        using var document = JsonDocument.Parse(
            input,
            new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            }
        );

        parser.VisitElement(document.RootElement);

        return parser.data;
    }

    private void VisitElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    EnterContext(property.Name);
                    VisitElement(property.Value);
                    ExitContext();
                }

                break;

            case JsonValueKind.Array:
                int index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    EnterContext(index.ToString(CultureInfo.InvariantCulture));
                    VisitElement(item);
                    ExitContext();
                    index++;
                }

                break;

            case JsonValueKind.String:
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                VisitPrimitive(element);
                break;

            default:
                throw new FormatException(
                    $"Unsupported JSON token '{element.ValueKind}' was found at '{currentPath}'."
                );
        }
    }

    private void VisitPrimitive(JsonElement element)
    {
        if (!data.TryAdd(currentPath, GetValue(element)))
        {
            throw new FormatException($"A duplicate key '{currentPath}' was found.");
        }
    }

    private static string? GetValue(JsonElement element) =>
        element.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.String => element.GetString(),
            JsonValueKind.True => bool.TrueString,
            JsonValueKind.False => bool.FalseString,
            _ => element.GetRawText(),
        };

    private void EnterContext(string name)
    {
        context.Push(name);
        currentPath = ConfigurationPath.Combine(context.Reverse());
    }

    private void ExitContext()
    {
        context.Pop();
        currentPath = ConfigurationPath.Combine(context.Reverse());
    }
}
