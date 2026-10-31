namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Computed $A7:CFC2 first-flame column lookup, indexed by the eight rain patterns rather than by individual flames.</summary>
public readonly struct PhantoonRainColumns : IReadOnlyList<int>
{
    /// <summary>Eight pattern-to-first-column entries; each pattern's caller subsequently emits eight flames across nine wrapping columns.</summary>
    public int Count => PhantoonPatternDefinitions.RainPlacements.Length;
    /// <summary>First flame's zero-based column immediately after the selected body's column, wrapping from eight to zero.</summary>
    /// <param name="pattern">Native rain-pattern index, 0..7.</param>
    /// <returns>A column from 0..8; following eight consecutive columns leaves the body placement's column empty.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The pattern is outside the eight-entry lookup.</exception>
    public int this[int pattern]
    {
        get
        {
            if ((uint)pattern >= Count) throw new ArgumentOutOfRangeException(nameof(pattern));
            int origin = PhantoonFlameSpawns.RainX(0);
            int spacing = PhantoonFlameSpawns.RainX(1) - origin;
            int bodyColumn = (PhantoonFlameRain.RainPlacement(pattern).Position.X - origin) / spacing;
            return (bodyColumn + 1) % PhantoonPatternDefinitions.RainColumnCount;
        }
    }
    /// <summary>Enumerates each pattern's first flame column in native pattern order, without advancing RNG or spawning flames.</summary>
    /// <returns>An enumerator over the eight computed column selectors.</returns>
    public IEnumerator<int> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same eight computed selectors through the non-generic collection interface.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
