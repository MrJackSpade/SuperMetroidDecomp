using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The installed artwork of the tileset a room loads: its characters, palette and pre-coloured block pictures.</summary>
/// <param name="Tileset">The tileset.</param>
/// <param name="CreCharacterCount">The common room elements' characters, bound first.</param>
/// <param name="TilesetCharacterCount">The tileset's characters, bound over them.</param>
/// <param name="PaletteBytes">The tileset's little-endian BGR555 palette bytes.</param>
/// <param name="Pictures">The installed pre-coloured room artwork: block sheets and library background images (#1392).</param>
public sealed record RoomTilesetArtwork(TilesetDefinition Tileset, int CreCharacterCount, int TilesetCharacterCount, byte[] PaletteBytes,
    InstalledRoomArtwork Pictures);
