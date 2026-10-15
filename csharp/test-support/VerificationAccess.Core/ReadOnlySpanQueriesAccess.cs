namespace SuperMetroid.Core;

/// <summary>Verification access to <see cref="ReadOnlySpanQueries"/> queries production does not use.</summary>
internal static class ReadOnlySpanQueriesAccess
{
    /// <summary>Allocating span queries that only verification runs.</summary>
    extension<T>(ReadOnlySpan<T> span)
    {
        /// <summary>A new array of <paramref name="selector"/>'s results given each element and its index, in element order.</summary>
        public TResult[] SelectToArray<TResult>(Func<T, int, TResult> selector)
        {
            Ensure.NotNull(selector);
            var result = new TResult[span.Length];
            for (int index = 0; index < span.Length; index++)
                result[index] = selector(span[index], index);
            return result;
        }
        /// <summary>A new array of this span's elements followed by <paramref name="other"/>'s.</summary>
        public T[] ConcatToArray(ReadOnlySpan<T> other)
        {
            var result = new T[span.Length + other.Length];
            span.CopyTo(result);
            other.CopyTo(result.AsSpan(span.Length));
            return result;
        }
        /// <summary>
        /// How many positions, up to the shorter length, hold unequal elements under <see cref="EqualityComparer{T}.Default"/>;
        /// pairs elements as <see cref="Enumerable.Zip{TFirst, TSecond}(IEnumerable{TFirst}, IEnumerable{TSecond})"/> does.
        /// </summary>
        public int CountDifferences(ReadOnlySpan<T> other)
        {
            int length = Math.Min(span.Length, other.Length);
            int count = 0;
            for (int index = 0; index < length; index++)
            {
                if (!EqualityComparer<T>.Default.Equals(span[index], other[index]))
                    count++;
            }
            return count;
        }
        /// <summary>The number of distinct elements under <see cref="EqualityComparer{T}.Default"/>.</summary>
        /// <remarks>Allocates one set of the distinct elements.</remarks>
        public int CountDistinct()
        {
            var distinct = new HashSet<T>(span.Length);
            foreach (T item in span)
                distinct.Add(item);
            return distinct.Count;
        }
        /// <summary>A new dictionary of the elements keyed by <paramref name="keySelector"/>; a duplicate key throws.</summary>
        public Dictionary<TKey, T> ToDictionary<TKey>(Func<T, TKey> keySelector)
            where TKey : notnull
        {
            Ensure.NotNull(keySelector);
            var result = new Dictionary<TKey, T>(span.Length);
            foreach (T item in span)
                result.Add(keySelector(item), item);
            return result;
        }
    }
}
