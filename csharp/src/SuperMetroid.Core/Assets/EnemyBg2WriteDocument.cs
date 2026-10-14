namespace SuperMetroid.Core.Assets;

/// <summary>A horizontal BG2 tile run in 32-by-64 tilemap coordinates.</summary>
[SpatialComponents("External format: the BG2 frame JSON stores each write destination as x/y tilemap cells.")]
public sealed record EnemyBg2WriteDocument
{
    /// <summary>Gets the zero-based destination column in the 32-tile-wide BG2 map.</summary>
    public required int X { get; init; }

    /// <summary>Gets the zero-based destination row in the 64-tile-high BG2 map.</summary>
    public required int Y { get; init; }

    /// <summary>Gets the contiguous native tilemap words written left to right from the destination.</summary>
    public required int[] Tiles { get; init; }
}
