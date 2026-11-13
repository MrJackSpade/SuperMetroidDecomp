using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

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
    /// applies the block's horizontal and vertical mirroring.
    /// </summary>
    /// <param name="appearance">The block shown and its mirroring.</param>
    /// <param name="blockDefinitions">The block compositions, <see cref="RoomMetatileFormat.TilesPerBlock"/> tiles each.</param>
    public static ExpandedBlockTiles Expand(RoomBlockAppearance appearance, ReadOnlySpan<BackgroundTile> blockDefinitions)
    {
        int blockIndex = appearance.Block;
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
        return appearance.Mirroring switch
        {
            BlockMirroring.None =>
                new ExpandedBlockTiles(topLeft, topRight, bottomLeft, bottomRight),
            BlockMirroring.Horizontal => new ExpandedBlockTiles(
                topRight.WithFlipsToggled(PictureFlip.Horizontal),
                topLeft.WithFlipsToggled(PictureFlip.Horizontal),
                bottomRight.WithFlipsToggled(PictureFlip.Horizontal),
                bottomLeft.WithFlipsToggled(PictureFlip.Horizontal)),
            BlockMirroring.Vertical => new ExpandedBlockTiles(
                bottomLeft.WithFlipsToggled(PictureFlip.Vertical),
                bottomRight.WithFlipsToggled(PictureFlip.Vertical),
                topLeft.WithFlipsToggled(PictureFlip.Vertical),
                topRight.WithFlipsToggled(PictureFlip.Vertical)),
            BlockMirroring.Horizontal | BlockMirroring.Vertical => new ExpandedBlockTiles(
                bottomRight.WithFlipsToggled(PictureFlip.Horizontal | PictureFlip.Vertical),
                bottomLeft.WithFlipsToggled(PictureFlip.Horizontal | PictureFlip.Vertical),
                topRight.WithFlipsToggled(PictureFlip.Horizontal | PictureFlip.Vertical),
                topLeft.WithFlipsToggled(PictureFlip.Horizontal | PictureFlip.Vertical)),
            _ => throw new InvalidDataException(
                $"Block appearance {appearance} has unsupported mirroring."),
        };
    }
}
