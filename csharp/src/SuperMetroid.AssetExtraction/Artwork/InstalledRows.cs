namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Where an installed colour file holds the row of an identity (#2556): its place in the written-out order the file lists
/// its rows in. Only an installed format's reader asks; nothing names a row by its number.
/// </summary>
internal static class InstalledRows
{
    /// <summary>The row <paramref name="order"/> gives <paramref name="identity"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The order does not list <paramref name="identity"/>.</exception>
    internal static int Of<T>(IReadOnlyList<T> order, T identity)
    {
        for (int row = 0; row < order.Count; row++)
        {
            if (EqualityComparer<T>.Default.Equals(order[row], identity))
                return row;
        }
        throw new ArgumentOutOfRangeException(nameof(identity), identity, "The installed rows hold no such identity.");
    }
}
