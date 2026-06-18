namespace System;

public static class StringExtensions
{
    extension(string source)
    {
        public string TrimEnd(ReadOnlySpan<char> value, StringComparison comparisonType)
        {
            ArgumentNullException.ThrowIfNull(source);

            if (value.IsEmpty)
            {
                return source;
            }

            return source.AsSpan().TrimEnd(value, comparisonType).ToString();
        }

        public string TrimStart(ReadOnlySpan<char> value, StringComparison comparisonType)
        {
            ArgumentNullException.ThrowIfNull(source);

            if (value.IsEmpty)
            {
                return source;
            }

            return source.AsSpan().TrimStart(value, comparisonType).ToString();
        }
    }

    extension(ReadOnlySpan<char> source)
    {
        public ReadOnlySpan<char> TrimEnd(ReadOnlySpan<char> value, StringComparison comparisonType)
        {
            if (value.IsEmpty)
            {
                return source;
            }

            var span = source;

            while (span.EndsWith(value, comparisonType))
            {
                span = span[..^value.Length];
            }

            return span;
        }

        public ReadOnlySpan<char> TrimStart(
            ReadOnlySpan<char> value,
            StringComparison comparisonType
        )
        {
            if (value.IsEmpty)
            {
                return source;
            }

            var span = source;

            while (span.StartsWith(value, comparisonType))
            {
                span = span[value.Length..];
            }

            return span;
        }
    }
}
