using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// The span queries only the import pipeline runs (#624): a first match and the arrays it builds from spans, without
/// copying a span to an array first. They keep <see cref="Enumerable"/>'s ordering and exception behavior; operations
/// whose result owns storage say so in their names and allocate exactly that result.
/// </summary>
internal static class ImportSpanQueries
{
    /// <summary>Queries over one span of <typeparamref name="T"/>.</summary>
    extension<T>(ReadOnlySpan<T> span)
    {
        /// <summary>The first element satisfying <paramref name="predicate"/>; throws when none does, as <see cref="Enumerable.First{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/> does.</summary>
        /// <exception cref="InvalidOperationException">No element satisfies the predicate.</exception>
        internal T First(Func<T, bool> predicate)
        {
            Ensure.NotNull(predicate);
            foreach (T item in span)
            {
                if (predicate(item))
                    return item;
            }
            throw new InvalidOperationException("Sequence contains no matching element");
        }

        /// <summary>A new array of <paramref name="selector"/>'s results, in element order. Allocates exactly that array.</summary>
        internal TResult[] SelectToArray<TResult>(Func<T, TResult> selector)
        {
            Ensure.NotNull(selector);
            var result = new TResult[span.Length];
            for (int index = 0; index < span.Length; index++)
                result[index] = selector(span[index]);
            return result;
        }

    }
}
