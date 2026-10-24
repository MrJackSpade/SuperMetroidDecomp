namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// One update of the Ceres Ridley getaway's floor that leaves its descriptor behind (#1338, #1406): which tile numbers it
/// copied, how many, and to which floor cell.
/// </summary>
/// <param name="TileSource">Where the copied tile numbers come from, 0 through 65535.</param>
/// <param name="TileCount">The tile numbers copied, 0 through 65535.</param>
/// <param name="FloorCell">The first floor cell they are copied to, 0 through 65535.</param>
[CompatibilityService("the descriptor a floor update leaves behind, which later delay over-reads see")]
public readonly record struct FloorUpdate(int TileSource, int TileCount, int FloorCell)
{
    /// <summary>Where the copied tile numbers come from.</summary>
    public int TileSource { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, TileSource);

    /// <summary>The tile numbers copied.</summary>
    public int TileCount { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, TileCount);

    /// <summary>The first floor cell they are copied to.</summary>
    public int FloorCell { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, FloorCell);
}
