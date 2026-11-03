using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>
/// The installed artwork of the tileset a room loads: its pre-coloured block pictures, which carry the tileset's colours
/// (#1392, #1512).
/// </summary>
/// <param name="Tileset">The tileset.</param>
/// <param name="Pictures">The installed pre-coloured room artwork: block sheets and library background images (#1392).</param>
public sealed record RoomTilesetArtwork(TilesetDefinition Tileset, InstalledRoomArtwork Pictures);
