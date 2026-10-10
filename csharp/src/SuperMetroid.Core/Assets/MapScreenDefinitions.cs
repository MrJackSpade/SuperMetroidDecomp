using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Presentation-only page identities and fixed map-screen geometry; no navigation state.</summary>
public static class MapScreenDefinitions
{
    /// <summary>Required schema revision for the named map-screen tilemap document.</summary>
    public const int Version = 1;
    /// <summary>JSON filename containing the world-map and per-area room-frame pages.</summary>
    public const string FileName = "map-screens.json";
    /// <summary>Stable page identity for the shared world-map foreground.</summary>
    public const string WorldForeground = "World.Foreground";
    /// <summary>Number of columns and rows in each complete SNES tilemap page.</summary>
    public const int PageColumns = 32, PageRows = 32;
    /// <summary>Number of tilemap cells in one 32-by-32 page.</summary>
    public const int PageCells = PageColumns * PageRows;
    /// <summary>Serialized byte count of one page of 16-bit tilemap words.</summary>
    public const int PageBytes = PageCells * sizeof(ushort);
    /// <summary>Number of playable Zebes areas represented by world and room pages.</summary>
    public const int ZebesAreas = 6;
    /// <summary>Total required page count: one foreground plus two pages per area.</summary>
    public const int PageCount = 1 + ZebesAreas * 2;

    /// <summary>Builds the stable world-map background page identity for an area.</summary>
    /// <param name="area">A playable Zebes area from zero through five.</param>
    /// <returns>The area's <c>World.*</c> page identity.</returns>
    public static string WorldBackground(AreaId area) => "World." + AreaName(area);
    /// <summary>Builds the stable room-map frame page identity for an area.</summary>
    /// <param name="area">A playable Zebes area from zero through five.</param>
    /// <returns>The area's <c>Room.*</c> page identity.</returns>
    public static string RoomFrame(AreaId area) => "Room." + AreaName(area);
    /// <summary>Returns the stable enum-name component used in a map page identity for a playable Zebes area.</summary>
    /// <param name="area">Area value to encode; only the six playable Zebes area values are supported.</param>
    /// <returns>The enum name for the supplied area.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value does not identify a playable Zebes area.</exception>
    private static string AreaName(AreaId area) => (uint)area < ZebesAreas
        ? area.ToString() : throw new ArgumentOutOfRangeException(nameof(area));

    /// <summary>Named pages select one of the authored PNG atlases, never a VRAM address.</summary>
    public static IEnumerable<(string Id, int TileColumns, int TileCount)> Pages()
    {
        yield return (WorldForeground, WorldMapArtworkFormat.TileColumns, WorldMapArtworkFormat.ForegroundTileCount);
        for (int area = 0; area < ZebesAreas; area++)
        {
            yield return (WorldBackground((AreaId)area), WorldMapArtworkFormat.TileColumns, WorldMapArtworkFormat.BackgroundTileCount);
            yield return (RoomFrame((AreaId)area), MapTileAtlasFormat.TileColumns, MapTileAtlasFormat.ByteCount / 32);
        }
    }
}

/// <summary>Map-world PNG identities, native source transfers, and compiled PPU destinations.</summary>
public static class WorldMapArtworkFormat
{
    /// <summary>Indexed PNG containing the shared 4-bpp world-map foreground characters.</summary>
    public const string ForegroundFile = "world-map-foreground.png";
    /// <summary>Indexed PNG containing the shared 2-bpp world-map background characters.</summary>
    public const string BackgroundFile = "world-map-background.png";
    /// <summary>Number of eight-pixel character columns in each exported atlas.</summary>
    public const int TileColumns = 16;
    /// <summary>Atlas width in pixels.</summary>
    public const int Width = TileColumns * 8;
    /// <summary>$81:8DDB LoadInitialMenuTiles transfers $5600 bytes from $8E:8000.</summary>
    public const int ForegroundSource = 0x8e8000;
    /// <summary>Size in bytes of the native 4-bpp foreground character transfer.</summary>
    public const int ForegroundBytes = 0x5600;
    /// <summary>Number of 32-byte 4-bpp characters in the foreground atlas.</summary>
    public const int ForegroundTileCount = ForegroundBytes / 32;
    /// <summary>Foreground atlas height in pixels.</summary>
    public const int ForegroundHeight = ForegroundTileCount / TileColumns * 8;
    /// <summary>$81:8DDB also installs $600 bytes of 2-bpp BG3 characters from $8E:D600.</summary>
    public const int BackgroundSource = 0x8ed600;
    /// <summary>Size in bytes of the native 2-bpp background character transfer.</summary>
    public const int BackgroundBytes = 0x600;
    /// <summary>Number of 16-byte 2-bpp characters in the background atlas.</summary>
    public const int BackgroundTileCount = BackgroundBytes / 16;
    /// <summary>Background atlas height in pixels.</summary>
    public const int BackgroundHeight = BackgroundTileCount / TileColumns * 8;
    /// <summary>Menu BG1 character base in byte-addressed VRAM.</summary>
    public const int ForegroundDestination = 0;
    /// <summary>Menu BG3 character base in byte-addressed VRAM, BG34NBA=$04.</summary>
    public const int BackgroundDestination = 0x8000;
}
