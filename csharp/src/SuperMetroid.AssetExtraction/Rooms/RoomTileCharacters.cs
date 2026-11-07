namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Where a room's load and Kraid's backdrop bind among the tile characters its blocks name (#1406).</summary>
public static class RoomTileCharacters
{
    /// <summary>The first of the room tileset's characters.</summary>
    public const int TilesetFirst = 0;

    /// <summary>The first of the common room elements' characters, which the tileset's may cover: the last 384.</summary>
    public const int CommonRoomElementsFirst = 640;

    /// <summary>The first of the room's tile characters Kraid's backdrop binds over.</summary>
    public const int KraidBackdropFirst = 1008;
}
