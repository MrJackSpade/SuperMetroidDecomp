namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>The kinds of thing a room block shows (#2100, #2560), which <see cref="RoomBlockAppearance.Shows"/> names.</summary>
public enum RoomBlockShown
{
    /// <summary>A block of the tileset's block set.</summary>
    Block,
    /// <summary>Nothing: open space.</summary>
    Nothing,
    /// <summary>No picture: terrain beyond the room's grid, which the room never shows.</summary>
    BeyondRoom,
    /// <summary>A block a collectible that shows its own picture writes under it, which its picture covers.</summary>
    CollectiblePairBlock,
}
