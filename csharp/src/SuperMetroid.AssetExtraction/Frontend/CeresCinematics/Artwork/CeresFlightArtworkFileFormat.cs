namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>File identities and exact geometry of the installed Ceres approach artwork.</summary>
public static class CeresFlightArtworkFileFormat
{
    /// <summary>
    /// Required schema revision of the paired front/rear map document: 2 names its views' tiles for the camera's views
    /// (#1634).
    /// </summary>
    public const int Version = 2;
    /// <summary>Indexed PNG filename for the shared gunship/Ceres chunky Mode-7 characters.</summary>
    public const string FloorFileName = "ceres-flight-mode7-characters.png";
    /// <summary>Import-data JSON filename for the two ordered approach map slices, distinct from the later destruction map slice (#1693).</summary>
    public const string MapFileName = "ceres-flight-mode7-maps.json";
    /// <summary>Indexed PNG filename for the space/Ceres planar OBJ characters and reused SPACE COLONY background characters.</summary>
    public const string ObjectFileName = "ceres-flight-object-characters.png";
    /// <summary>Mode-7 PNG width in pixels: sixteen 8-pixel character columns.</summary>
    public const int FloorWidth = 128;
    /// <summary>Mode-7 PNG height in pixels: sixteen 8-pixel character rows.</summary>
    public const int FloorHeight = 128;
    /// <summary>The chunky 8-bpp pixels of one Mode 7 floor character.</summary>
    public const int FloorCharacterBytes = 64;
    /// <summary>Exact chunky Mode-7 character length, 16384 bytes for 256 characters at 64 bytes each.</summary>
    public const int FloorByteCount = 256 * FloorCharacterBytes;
    /// <summary>OBJ PNG width in pixels: thirty-two 8-pixel character columns.</summary>
    public const int ObjectWidth = 256;
    /// <summary>OBJ PNG height in pixels: sixteen 8-pixel character rows.</summary>
    public const int ObjectHeight = 128;
    /// <summary>Number of tile cells per row in each view's editing-array layout, not the full Mode-7 map stride.</summary>
    public const int MapWidth = 32;
    /// <summary>Number of tile-cell rows in each view's editing-array layout.</summary>
    public const int MapHeight = 24;
    /// <summary>Number of one-byte tile indices in each front or rear tile slice: 768.</summary>
    public const int MapCellsPerView = MapWidth * MapHeight;
    /// <summary>The cells of the front and rear views together, front first.</summary>
    public const int MapCellCount = 2 * MapCellsPerView;
    /// <summary>The destruction's shared sprite characters loaded over the start of the station's: 208 of them.</summary>
    public const int SharedObjectCharacterBytes = 208 * ObjectCharacterBytes;
    /// <summary>The planar bytes of one 4-bpp OBJ character.</summary>
    public const int ObjectCharacterBytes = 32;
    /// <summary>Exact planar 4-bpp OBJ character length, 16384 bytes for 512 characters at 32 bytes each.</summary>
    public const int ObjectByteCount = 512 * ObjectCharacterBytes;
}
