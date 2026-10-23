using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Presentation-only page identities and fixed map-screen geometry; no navigation state.</summary>
public static class MapScreenDefinitions
{
    /// <summary>Required schema revision for the named map-screen tilemap document.</summary>
    public const int Version = 1;
    /// <summary>JSON filename containing the world-map and per-area room-frame pages.</summary>
    public const string FileName = "map-screens.json";
    /// <summary>The shared world-map foreground page.</summary>
    public static MapScreenPageId WorldForeground => MapScreenPageId.WorldForeground.Instance;
    /// <summary>Number of columns and rows in each complete SNES tilemap page.</summary>
    public const int PageColumns = 32, PageRows = 32;
    /// <summary>The sprite row the map's highlight colour cycle colours.</summary>
    public const int HighlightSpriteRow = 3;
    /// <summary>Number of tilemap cells in one 32-by-32 page.</summary>
    public const int PageCells = PageColumns * PageRows;
    /// <summary>Serialized byte count of one page of 16-bit tilemap words.</summary>
    public const int PageBytes = PageCells * sizeof(ushort);
    /// <summary>Number of playable Zebes areas represented by world and room pages.</summary>
    public const int ZebesAreas = 6;
    /// <summary>Total required page count: one foreground plus two pages per area.</summary>
    public const int PageCount = 1 + ZebesAreas * 2;

    /// <summary>The world-map background page of an area.</summary>
    /// <param name="area">A playable Zebes area from zero through five.</param>
    public static MapScreenPageId WorldBackground(AreaId area) => new MapScreenPageId.WorldBackground(area);
    /// <summary>The room-map frame page of an area.</summary>
    /// <param name="area">A playable Zebes area from zero through five.</param>
    public static MapScreenPageId RoomFrame(AreaId area) => new MapScreenPageId.RoomFrame(area);
    /// <summary>Returns <paramref name="area"/> when it is a playable Zebes area, rejecting menu or other non-area identities.</summary>
    internal static AreaId RequireZebesArea(AreaId area) => (uint)area < ZebesAreas
        ? area : throw new ArgumentOutOfRangeException(nameof(area));

    /// <summary>Named pages select one of the authored PNG atlases, never a VRAM address.</summary>
    public static IEnumerable<MapScreenPage> Pages()
    {
        yield return new(WorldForeground, WorldMapArtworkFormat.TileColumns, WorldMapArtworkFormat.ForegroundTileCount);
        for (int area = 0; area < ZebesAreas; area++)
        {
            yield return new(WorldBackground((AreaId)area), WorldMapArtworkFormat.TileColumns, WorldMapArtworkFormat.BackgroundTileCount);
            yield return new(RoomFrame((AreaId)area), MapTileAtlasFormat.TileColumns, MapTileAtlasFormat.ByteCount / 32);
        }
    }
}
