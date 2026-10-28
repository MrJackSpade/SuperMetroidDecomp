using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The four tiles a 16x16 block shows, row by row, already mirrored by the block's level word.</summary>
/// <param name="TopLeft">The upper-left tile.</param>
/// <param name="TopRight">The upper-right tile.</param>
/// <param name="BottomLeft">The lower-left tile.</param>
/// <param name="BottomRight">The lower-right tile.</param>
public readonly record struct ExpandedBlockTiles(
    BackgroundTile TopLeft,
    BackgroundTile TopRight,
    BackgroundTile BottomLeft,
    BackgroundTile BottomRight);
