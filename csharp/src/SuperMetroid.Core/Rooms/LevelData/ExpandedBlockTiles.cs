namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Four row-major SNES tilemap words forming a visual 16x16 block.</summary>
public readonly record struct ExpandedBlockTiles(
    ushort TopLeft,
    ushort TopRight,
    ushort BottomLeft,
    ushort BottomRight);
