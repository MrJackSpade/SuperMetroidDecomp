using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The installed artwork of the tileset a room loads: its characters, palette and pre-coloured block pictures.</summary>
/// <param name="Tileset">The tileset.</param>
/// <param name="CreCharacters">The common room elements' planar BG characters, uploaded first.</param>
/// <param name="RoomCharacters">The tileset's planar BG characters, uploaded over them.</param>
/// <param name="PaletteBytes">The tileset's little-endian BGR555 palette bytes.</param>
/// <param name="BlockSheet">The pre-coloured pictures of the tileset's blocks (#1392).</param>
public sealed record RoomTilesetArtwork(TilesetDefinition Tileset, byte[] CreCharacters, byte[] RoomCharacters, byte[] PaletteBytes,
    RoomBlockSheet BlockSheet);
