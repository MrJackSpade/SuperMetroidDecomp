using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Names the ROM-derived pieces required to render one static room state. Keeping filenames
/// here, rather than hiding them in the renderer, makes the eventual room-state table explicit.
/// </summary>
public sealed record RoomRenderDefinition(
    string Name,
    Extent16<RoomScreens> Size,
    string LevelData,
    string AreaTiles,
    string AreaTileTable,
    string Palette,
    string CommonTiles = "CRE_Tiles_Compressed.bin",
    string CommonTileTable = "CRE_TileTable_Compressed.bin");
