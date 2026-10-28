namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>A run of consecutive world-map character cells, both ends included.</summary>
/// <param name="First">The run's first cell.</param>
/// <param name="Last">The run's last cell.</param>
internal readonly record struct WorldMapCellRange(int First, int Last)
{
    /// <summary>Whether <paramref name="cell"/> lies in the run.</summary>
    internal bool Contains(int cell) => cell >= First && cell <= Last;
}
