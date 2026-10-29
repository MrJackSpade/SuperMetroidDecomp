using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The installed artwork of the tileset a room loads: its colours and pre-coloured block pictures.</summary>
/// <param name="Tileset">The tileset.</param>
/// <param name="Colours">The tileset's colours for each background set (#1464).</param>
/// <param name="Pictures">The installed pre-coloured room artwork: block sheets and library background images (#1392).</param>
public sealed record RoomTilesetArtwork(TilesetDefinition Tileset, ShownBackgroundColourSets Colours, InstalledRoomArtwork Pictures);
