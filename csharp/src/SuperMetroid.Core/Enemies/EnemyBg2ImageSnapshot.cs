using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// An enemy BG2 image as an accepted NMI published it (#1353): its tiles in the hardware's screen order, shown on BG2
/// through the map size BG2's registers select, as the uploaded image was.
/// </summary>
/// <param name="tiles">The image's tiles, row by row, 32 per row: screen order for any 32-wide or two-screen map.</param>
public sealed class EnemyBg2ImageSnapshot(ImmutableArray<BackgroundTile> tiles)
{
    /// <summary>The image's tiles, row by row, 32 per row.</summary>
    public ImmutableArray<BackgroundTile> Tiles { get; } = tiles;
}
