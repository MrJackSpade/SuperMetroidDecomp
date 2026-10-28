namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>The artwork resources a room state's native graphics set selects.</summary>
public static class RoomTilesets
{
    /// <summary>Selects block, character and palette resources for the room state's native graphics set.</summary>
    /// <exception cref="InvalidDataException"><paramref name="graphicsSet"/> is outside the retail tileset table.</exception>
    public static TilesetDefinition Get(int graphicsSet) =>
        (uint)graphicsSet < RoomTilesetDefinitions.Count
            ? RoomTilesetDefinitions.Tilesets[graphicsSet]
            : throw new InvalidDataException($"Graphics set ${graphicsSet:X2} is outside the compiled retail tileset table.");
}
