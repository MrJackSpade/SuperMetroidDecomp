using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Expands one 16x16 level block into the four 8x8 tiles it shows.
/// </summary>
/// <remarks>
/// This is the common inner operation in the column streamer at <c>$80:AA95</c>, the row
/// streamer at <c>$80:AC57</c>, and the desktop room renderer. Centralizing it prevents the
/// live tilemap and exported PNG paths from acquiring subtly different flip semantics.
/// </remarks>
public static class LevelBlockTilemapExpander
{
    /// <summary>
    /// Reads the indexed four-tile definition from the combined CRE/area block table and
    /// applies the parent level entry's horizontal and vertical flips.
    /// </summary>
    /// <param name="levelEntry">The level word naming the block definition and its mirroring.</param>
    /// <param name="blockDefinitions">The block compositions, <see cref="RoomMetatileFormat.TilesPerBlock"/> tiles each.</param>
    public static ExpandedBlockTiles Expand(RoomLevelWord levelEntry, ReadOnlySpan<BackgroundTile> blockDefinitions)
    {
        // The typed word keeps the collision nibble explicitly out of visual expansion.
        int blockIndex = levelEntry.VisualBlockIndex;
        int first = blockIndex * RoomMetatileFormat.TilesPerBlock;
        if (first + RoomMetatileFormat.TilesPerBlock > blockDefinitions.Length)
        {
            throw new InvalidDataException(
                $"Block index ${blockIndex:X3} exceeds the {blockDefinitions.Length / RoomMetatileFormat.TilesPerBlock} loaded definitions.");
        }

        BackgroundTile topLeft = blockDefinitions[first];
        BackgroundTile topRight = blockDefinitions[first + 1];
        BackgroundTile bottomLeft = blockDefinitions[first + 2];
        BackgroundTile bottomRight = blockDefinitions[first + 3];

        // Mirroring does two jobs at once: the four children move to their mirrored quadrants,
        // and each 8x8 child's own mirroring toggles so pixels mirror inside that child.
        // The case order mirrors the native horizontal/vertical flag comparisons.
        return levelEntry.VisualFlipFlags switch
        {
            LevelBlockFlipFlags.None =>
                new ExpandedBlockTiles(topLeft, topRight, bottomLeft, bottomRight),
            LevelBlockFlipFlags.Horizontal => new ExpandedBlockTiles(
                topRight.WithFlipsToggled(TileFlip.Horizontal),
                topLeft.WithFlipsToggled(TileFlip.Horizontal),
                bottomRight.WithFlipsToggled(TileFlip.Horizontal),
                bottomLeft.WithFlipsToggled(TileFlip.Horizontal)),
            LevelBlockFlipFlags.Vertical => new ExpandedBlockTiles(
                bottomLeft.WithFlipsToggled(TileFlip.Vertical),
                bottomRight.WithFlipsToggled(TileFlip.Vertical),
                topLeft.WithFlipsToggled(TileFlip.Vertical),
                topRight.WithFlipsToggled(TileFlip.Vertical)),
            LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical => new ExpandedBlockTiles(
                bottomRight.WithFlipsToggled(TileFlip.Horizontal | TileFlip.Vertical),
                bottomLeft.WithFlipsToggled(TileFlip.Horizontal | TileFlip.Vertical),
                topRight.WithFlipsToggled(TileFlip.Horizontal | TileFlip.Vertical),
                topLeft.WithFlipsToggled(TileFlip.Horizontal | TileFlip.Vertical)),
            _ => throw new InvalidDataException(
                $"Level word ${levelEntry.Raw:X4} contains unsupported visual flip flags."),
        };
    }
}
