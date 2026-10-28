namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// A read-only view of one native casual-flame table: flame count, cooldown, then one interval per flame.
/// </summary>
/// <param name="words">The table's words in forward storage order.</param>
public readonly struct PhantoonCasualFlameSchedule(ushort[] words) : IReadOnlyList<ushort>
{
    /// <summary>Number of stored words, including the two header words; not the number of flames.</summary>
    public int Count => words.Length;

    /// <summary>Reads a native table word: flame count at 0, 180-update cooldown at 1, or an interval thereafter.</summary>
    /// <param name="index">Zero-based word index, less than <see cref="Count"/>.</param>
    /// <returns>The unsigned count or scheduler-update duration at the requested position.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is negative or at least <see cref="Count"/>.</exception>
    public ushort this[int index] =>
        (uint)index < (uint)words.Length ? words[index] : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>Enumerates the header and interval words in forward storage order, not chronological flame order.</summary>
    /// <returns>An enumerator over all <see cref="Count"/> words without exposing mutable storage.</returns>
    public IEnumerator<ushort> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }

    /// <summary>Exposes the same native storage order through non-generic enumeration.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
