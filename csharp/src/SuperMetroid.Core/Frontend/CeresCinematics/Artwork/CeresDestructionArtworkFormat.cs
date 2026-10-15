namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>Files and exact native transfer dimensions for the destruction/reveal art.</summary>
public static class CeresDestructionArtworkFormat
{
    /// <summary>Supported schema revision for the Ceres destruction Mode 7 map document.</summary>
    public const int Version = 1;
    /// <summary>Installed JSON filename for the two destruction map views followed by the native clear-map slice.</summary>
    public const string CeresMapFileName = "ceres-destruction-mode7-maps.json";
    /// <summary>Installed JSON filename for the ordinary 32-by-32 BG tilemap page shown during the Zebes reveal.</summary>
    public const string ZebesMapFileName = "zebes-reveal-tilemap.json";
    /// <summary>Installed indexed PNG filename for the Zebes-reveal 4bpp characters, separate from its tilemap references.</summary>
    public const string ZebesCharacterFileName = "zebes-reveal-characters.png";
    /// <summary>Number of eight-bit Mode 7 map cells per row in each exported Ceres slice.</summary>
    public const int MapWidth = 32;
    /// <summary>Number of map-cell rows in each exported Ceres slice.</summary>
    public const int MapHeight = 24;
    /// <summary>768 row-major cells per Ceres view, each compiling to one map-lane byte.</summary>
    public const int CellsPerView = MapWidth * MapHeight;
    /// <summary>Three ordered Ceres slices from decompressed source offsets $600, $900, and $C00; the last clears the scene.</summary>
    public const int ViewCount = 3;
    /// <summary>$0900 compiled map-lane bytes for all three slices, appended after the shared $0600-byte Ceres-flight maps.</summary>
    public const int MapByteCount = ViewCount * CellsPerView;
    /// <summary>$0800 bytes for one ordinary BG tilemap page, transferred from native ZebesTilemap to VRAM word $5C00 by $8B:C6F1–$C70E.</summary>
    public const int ZebesMapByteCount = 1024 * sizeof(ushort);
    /// <summary>$4000 bytes of 4bpp Zebes characters, matching the native ZebesTiles transfer to VRAM word $6000 at $8B:C70C–$C72E.</summary>
    public const int ZebesCharacterByteCount = 512 * 32;
}
