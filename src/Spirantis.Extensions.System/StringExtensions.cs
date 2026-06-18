namespace System;

/// <summary>
/// Extension methods for trimming whole substrings (not just individual characters)
/// from <see cref="string"/> and <see cref="ReadOnlySpan{T}"/> of <see cref="char"/>.
/// </summary>
public static class StringExtensions
{
    extension(string source)
    {
        /// <summary>Removes all trailing occurrences of <paramref name="value"/>.</summary>
        /// <param name="value">The substring to remove from the end.</param>
        /// <param name="comparisonType">The comparison used to match <paramref name="value"/>.</param>
        /// <returns>The trimmed string, or the original when <paramref name="value"/> is empty.</returns>
        public string TrimEnd(ReadOnlySpan<char> value, StringComparison comparisonType)
        {
            ArgumentNullException.ThrowIfNull(source);

            if (value.IsEmpty)
            {
                return source;
            }

            return source.AsSpan().TrimEnd(value, comparisonType).ToString();
        }

        /// <summary>Removes all leading occurrences of <paramref name="value"/>.</summary>
        /// <param name="value">The substring to remove from the start.</param>
        /// <param name="comparisonType">The comparison used to match <paramref name="value"/>.</param>
        /// <returns>The trimmed string, or the original when <paramref name="value"/> is empty.</returns>
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
        /// <summary>Removes all trailing occurrences of <paramref name="value"/>.</summary>
        /// <param name="value">The substring to remove from the end.</param>
        /// <param name="comparisonType">The comparison used to match <paramref name="value"/>.</param>
        /// <returns>The trimmed span, or the original when <paramref name="value"/> is empty.</returns>
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

        /// <summary>Removes all leading occurrences of <paramref name="value"/>.</summary>
        /// <param name="value">The substring to remove from the start.</param>
        /// <param name="comparisonType">The comparison used to match <paramref name="value"/>.</param>
        /// <returns>The trimmed span, or the original when <paramref name="value"/> is empty.</returns>
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
