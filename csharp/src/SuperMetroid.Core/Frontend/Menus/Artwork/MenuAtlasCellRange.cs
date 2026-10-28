namespace SuperMetroid.Core.Frontend.Menus.Artwork;

/// <summary>An inclusive run of eight-pixel cells in the menu character atlas, numbered as the native tilemaps number them.</summary>
/// <param name="First">The run's first atlas cell.</param>
/// <param name="Last">The run's last atlas cell.</param>
internal readonly record struct MenuAtlasCellRange(int First, int Last)
{
    /// <summary>A run of one cell.</summary>
    internal MenuAtlasCellRange(int cell) : this(cell, cell)
    {
    }

    /// <summary>Whether <paramref name="cell"/> lies in this run.</summary>
    internal bool Contains(int cell) => cell >= First && cell <= Last;

    /// <summary>Whether <paramref name="cell"/> lies in any of <paramref name="ranges"/>.</summary>
    /// <remarks>Artwork loaders ask this for every pixel, so it walks the runs without allocating.</remarks>
    internal static bool AnyContains(IReadOnlyList<MenuAtlasCellRange> ranges, int cell)
    {
        for (int index = 0; index < ranges.Count; index++)
        {
            if (ranges[index].Contains(cell))
                return true;
        }
        return false;
    }
}
