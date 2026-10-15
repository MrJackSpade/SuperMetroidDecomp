namespace SuperMetroid.Core;

/// <summary>
/// Queries over a <see cref="ReadOnlySpan{T}"/> without copying it to an array first (#624). Scalar queries iterate the
/// span directly and allocate nothing beyond the caller's delegate; they keep <see cref="Enumerable"/>'s ordering,
/// short-circuiting and exception behavior. Operations whose result owns storage say so in their names and allocate
/// exactly that result.
/// </summary>
/// <remarks>
/// With C# 14's first-class span conversions, arrays bind to these queries too, in preference to their
/// <see cref="Enumerable"/> counterparts; the results are identical.
/// </remarks>
public static class ReadOnlySpanQueries
{
    /// <summary>Queries over one span of <typeparamref name="T"/>.</summary>
    extension<T>(ReadOnlySpan<T> span)
    {
        /// <summary>Whether any element satisfies <paramref name="predicate"/>, stopping at the first that does.</summary>
        public bool Any(Func<T, bool> predicate)
        {
            Ensure.NotNull(predicate);
            foreach (T item in span)
            {
                if (predicate(item))
                    return true;
            }
            return false;
        }

        /// <summary>Whether every element satisfies <paramref name="predicate"/>, stopping at the first that does not.</summary>
        public bool All(Func<T, bool> predicate)
        {
            Ensure.NotNull(predicate);
            foreach (T item in span)
            {
                if (!predicate(item))
                    return false;
            }
            return true;
        }

        /// <summary>The number of elements satisfying <paramref name="predicate"/>.</summary>
        public int Count(Func<T, bool> predicate)
        {
            Ensure.NotNull(predicate);
            int count = 0;
            foreach (T item in span)
            {
                if (predicate(item))
                    count = checked(count + 1);
            }
            return count;
        }

        /// <summary>The first element satisfying <paramref name="predicate"/>; throws when none does, as <see cref="Enumerable.First{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/> does.</summary>
        public T First(Func<T, bool> predicate)
        {
            Ensure.NotNull(predicate);
            foreach (T item in span)
            {
                if (predicate(item))
                    return item;
            }
            throw new InvalidOperationException("Sequence contains no matching element");
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

        /// <summary>The checked sum of <paramref name="selector"/> over the elements; overflow throws, as in <see cref="Enumerable"/>.</summary>
        public int Sum(Func<T, int> selector)
        {
            Ensure.NotNull(selector);
            int sum = 0;
            foreach (T item in span)
                sum = checked(sum + selector(item));
            return sum;
        }

        /// <summary>A new array of <paramref name="selector"/>'s results, in element order. Allocates exactly that array.</summary>
        public TResult[] SelectToArray<TResult>(Func<T, TResult> selector)
        {
            Ensure.NotNull(selector);
            var result = new TResult[span.Length];
            for (int index = 0; index < span.Length; index++)
                result[index] = selector(span[index]);
            return result;
        }

        /// <summary>A new array of the elements satisfying <paramref name="predicate"/>, in element order.</summary>
        /// <remarks>Counts first, then fills one exactly sized array, so <paramref name="predicate"/> runs twice per element.</remarks>
        public T[] WhereToArray(Func<T, bool> predicate)
        {
            Ensure.NotNull(predicate);
            var result = new T[span.Count(predicate)];
            int next = 0;
            foreach (T item in span)
            {
                if (predicate(item))
                    result[next++] = item;
            }
            return result;
        }


        /// <summary>A new dictionary keyed by <paramref name="keySelector"/>; a duplicate key throws, as in <see cref="Enumerable"/>.</summary>
        public Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> valueSelector)
            where TKey : notnull => span.ToDictionary(keySelector, valueSelector, comparer: null);

        /// <summary>A new dictionary keyed by <paramref name="keySelector"/> under <paramref name="comparer"/>; a duplicate key throws.</summary>
        public Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> valueSelector,
            IEqualityComparer<TKey>? comparer)
            where TKey : notnull
        {
            Ensure.NotNull(keySelector);
            Ensure.NotNull(valueSelector);
            var result = new Dictionary<TKey, TValue>(span.Length, comparer);
            foreach (T item in span)
                result.Add(keySelector(item), valueSelector(item));
            return result;
        }

    }
}
