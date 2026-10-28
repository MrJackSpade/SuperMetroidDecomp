namespace SuperMetroid.Core;

/// <summary>Development-tool span queries; never linked by player hosts.</summary>
internal static class ReadOnlySpanQueriesToolingExtensions
{
    /// <summary>The index-aware filter and single-match query only development tools run.</summary>
    extension<T>(ReadOnlySpan<T> span)
    {
        /// <summary>A new array of the elements satisfying <paramref name="predicate"/> given each element and its index.</summary>
        public T[] WhereToArray(Func<T, int, bool> predicate)
        {
            Ensure.NotNull(predicate);
            int count = 0;
            for (int index = 0; index < span.Length; index++)
            {
                if (predicate(span[index], index))
                    count++;
            }
            var result = new T[count];
            int next = 0;
            for (int index = 0; index < span.Length; index++)
            {
                if (predicate(span[index], index))
                    result[next++] = span[index];
            }
            return result;
        }

        /// <summary>The only element satisfying <paramref name="predicate"/>; throws when none or several do.</summary>
        public T Single(Func<T, bool> predicate)
        {
            Ensure.NotNull(predicate);
            int match = -1;
            for (int index = 0; index < span.Length; index++)
            {
                if (!predicate(span[index]))
                    continue;
                if (match >= 0)
                    throw new InvalidOperationException("Sequence contains more than one matching element");
                match = index;
            }
            return match >= 0 ? span[match] : throw new InvalidOperationException("Sequence contains no matching element");
        }
    }
}
