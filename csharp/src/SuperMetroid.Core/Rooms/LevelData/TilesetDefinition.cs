namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>The artwork a graphics set draws its rooms with.</summary>
/// <param name="Blocks">The block table its rooms are built from.</param>
/// <param name="Characters">The background character sheet uploaded after the common room elements.</param>
/// <param name="Palette">The background palette; its first 128 colours are loaded with the room.</param>
public readonly record struct TilesetDefinition(
    RoomBlockSet Blocks,
    RoomCharacterSet Characters,
    RoomPaletteSet Palette);
