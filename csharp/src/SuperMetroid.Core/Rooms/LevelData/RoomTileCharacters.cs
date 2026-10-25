namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Where a room's load and its collectibles bind among the tile characters its blocks name (#1406).</summary>
public static class RoomTileCharacters
{
    /// <summary>The first of the room tileset's characters.</summary>
    public const int TilesetFirst = 0;

    /// <summary>The tile characters a room's blocks can name.</summary>
    private const int Characters = 1024;

    /// <summary>The first of the common room elements' characters, which the tileset's may cover: the last 384.</summary>
    public const int CommonRoomElementsFirst = 640;

    /// <summary>The collectible slots the room's collectibles rotate through.</summary>
    private const int CollectibleSlots = 4;

    /// <summary>The characters one collectible slot holds: two animation frames of four.</summary>
    public const int CollectibleSlotCharacters = 8;

    /// <summary>The first of the four collectible slots instruction <c>$84:8764</c> rotates through: the last 32 characters.</summary>
    public const int CollectiblesFirst = Characters - CollectibleSlots * CollectibleSlotCharacters;
}
