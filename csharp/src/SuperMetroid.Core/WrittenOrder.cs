using System.Collections;

namespace SuperMetroid.Core;

/// <summary>
/// A written-out order of identities (#2567): the order data states for them, never the order their type declares its
/// members in or a dictionary enumerates its keys in. It lists the identities in that order and gives each its place, so a
/// sort by place follows the data and reordering a declaration changes nothing.
/// </summary>
/// <typeparam name="T">The identities ordered.</typeparam>
public sealed partial class WrittenOrder<T> : IReadOnlyList<T> where T : notnull
{
    /// <summary>The identities in order.</summary>
    private readonly T[] members;

    /// <summary>Each identity's place in the order.</summary>
    private readonly Dictionary<T, int> places;

    /// <summary>Takes <paramref name="order"/> as the order, each identity listed once.</summary>
    /// <param name="order">The identities in the order the data states.</param>
    /// <exception cref="ArgumentException">An identity is listed twice.</exception>
    public WrittenOrder(IReadOnlyList<T> order)
    {
        members = [.. Ensure.NotNull(order)];
        places = new Dictionary<T, int>(members.Length);
        for (int place = 0; place < members.Length; place++)
        {
            if (!places.TryAdd(members[place], place))
                throw new ArgumentException($"The order lists {members[place]} twice.", nameof(order));
        }
    }

    /// <inheritdoc/>
    public int Count => members.Length;

    /// <inheritdoc/>
    public T this[int index] => members[index];

    /// <summary>The place of <paramref name="member"/> in the order, counted from zero.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The order does not list <paramref name="member"/>.</exception>
    public int PlaceOf(T member) => Ensure.Listed(places, member);

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)members).GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
