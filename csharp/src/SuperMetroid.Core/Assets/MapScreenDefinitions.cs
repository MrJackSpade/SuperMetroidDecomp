using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Presentation-only page identities and fixed map-screen geometry; no navigation state.</summary>
public static class MapScreenDefinitions
{
    public const int Version = 1;
    public const string FileName = "map-screens.json";
    public const string WorldForeground = "World.Foreground";
    public const int PageColumns = 32, PageRows = 32;
    public const int PageCells = PageColumns * PageRows;
    public const int PageBytes = PageCells * sizeof(ushort);
    public const int ZebesAreas = 6;
    public const int PageCount = 1 + ZebesAreas * 2;

    public static string WorldBackground(AreaId area) => "World." + AreaName(area);
    public static string RoomFrame(AreaId area) => "Room." + AreaName(area);
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
    public const string ForegroundFile = "world-map-foreground.png";
    public const string BackgroundFile = "world-map-background.png";
    public const int TileColumns = 16;
    public const int Width = TileColumns * 8;
    /// <summary>$81:8DDB LoadInitialMenuTiles transfers $5600 bytes from $8E:8000.</summary>
    public const int ForegroundSource = 0x8e8000;
    public const int ForegroundBytes = 0x5600;
    public const int ForegroundTileCount = ForegroundBytes / 32;
    public const int ForegroundHeight = ForegroundTileCount / TileColumns * 8;
    /// <summary>$81:8DDB also installs $600 bytes of 2-bpp BG3 characters from $8E:D600.</summary>
    public const int BackgroundSource = 0x8ed600;
    public const int BackgroundBytes = 0x600;
    public const int BackgroundTileCount = BackgroundBytes / 16;
    public const int BackgroundHeight = BackgroundTileCount / TileColumns * 8;
    /// <summary>Menu BG1 character base in byte-addressed VRAM.</summary>
    public const int ForegroundDestination = 0;
    /// <summary>Menu BG3 character base in byte-addressed VRAM, BG34NBA=$04.</summary>
    public const int BackgroundDestination = 0x8000;
}
