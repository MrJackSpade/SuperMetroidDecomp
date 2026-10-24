namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Where a room's load and its collectibles bind among the tile characters its blocks name (#1406).</summary>
public static class RoomTileCharacters
{
    /// <summary>The first of the room tileset's characters.</summary>
    public const int TilesetFirst = 0;

    /// <summary>The first of the common room elements' characters, which the tileset's may cover.</summary>
    public const int CommonRoomElementsFirst = 0x280;

    /// <summary>The first of the four collectible slots instruction <c>$84:8764</c> rotates through.</summary>
    public const int CollectiblesFirst = 0x3e0;

    /// <summary>The characters one collectible slot holds: two animation frames of four.</summary>
    public const int CollectibleSlotCharacters = 8;
}
