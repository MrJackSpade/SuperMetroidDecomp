namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Four row-major SNES tilemap words forming a visual 16x16 block.</summary>
/// <param name="TopLeft">The upper-left 8x8 character's tilemap entry.</param>
/// <param name="TopRight">The upper-right 8x8 character's tilemap entry.</param>
/// <param name="BottomLeft">The lower-left 8x8 character's tilemap entry.</param>
/// <param name="BottomRight">The lower-right 8x8 character's tilemap entry.</param>
public readonly record struct ExpandedBlockTiles(
    SnesBgTilemapWord TopLeft,
    SnesBgTilemapWord TopRight,
    SnesBgTilemapWord BottomLeft,
    SnesBgTilemapWord BottomRight);
