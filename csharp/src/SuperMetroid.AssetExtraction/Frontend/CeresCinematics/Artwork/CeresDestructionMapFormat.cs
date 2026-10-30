namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>File and exact native transfer dimensions of the destruction's Mode 7 maps (#1467).</summary>
public static class CeresDestructionMapFormat
{
    /// <summary>Supported schema revision for the Ceres destruction Mode 7 map document.</summary>
    public const int Version = 1;
    /// <summary>Installed JSON filename for the two destruction map views followed by the native clear-map slice.</summary>
    public const string FileName = "ceres-destruction-mode7-maps.json";
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
}
