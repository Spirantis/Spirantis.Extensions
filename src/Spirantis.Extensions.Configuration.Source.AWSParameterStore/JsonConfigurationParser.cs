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

    public static IDictionary<string, string?> Parse(string input, string basePath)
    {
        var parser = new JsonConfigurationParser();

        using var document = JsonDocument.Parse(
            input,
            new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            }
        );

        parser.VisitElement(document.RootElement, basePath);

        return parser.data;
    }

    private void VisitElement(JsonElement element, string path)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    VisitElement(property.Value, ConfigurationPath.Combine(path, property.Name));
                }

                break;

            case JsonValueKind.Array:
                int index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    string key = index.ToString(CultureInfo.InvariantCulture);
                    VisitElement(item, ConfigurationPath.Combine(path, key));
                    index++;
                }

                break;

            case JsonValueKind.String:
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                if (!data.TryAdd(path, GetValue(element)))
                {
                    throw new FormatException($"A duplicate key '{path}' was found.");
                }

                break;

            default:
                throw new FormatException(
                    $"Unsupported JSON token '{element.ValueKind}' was found at '{path}'."
                );
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
}
