namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The artwork a graphics set draws its rooms with (#1466): the graphics set, which names the tileset's pictures, the
/// block table its level words use and the palette its rooms load. Which characters the set's pictures are composed from
/// is the import's knowledge.
/// </summary>
/// <param name="GraphicsSet">The graphics set: the tileset's identity, which names its pictures.</param>
/// <param name="Blocks">The block table its rooms are built from.</param>
/// <param name="Palette">The background palette; its first 128 colours are loaded with the room.</param>
public readonly record struct TilesetDefinition(
    RoomGraphicsSet GraphicsSet,
    RoomBlockSet Blocks,
    RoomPaletteSet Palette);
