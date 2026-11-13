namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The artwork a graphics set draws its rooms with (#1466): the graphics set, which names the tileset's pictures. Which
/// source artwork, block table and palette the set's pictures are composed from is the import's knowledge (#1616).
/// </summary>
/// <param name="GraphicsSet">The graphics set: the tileset's identity, which names its pictures.</param>
public readonly record struct TilesetDefinition(RoomGraphicsSet GraphicsSet);
