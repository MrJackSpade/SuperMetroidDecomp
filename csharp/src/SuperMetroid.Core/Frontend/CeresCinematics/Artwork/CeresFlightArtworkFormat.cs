namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>File identities and exact Ceres approach artwork geometry.</summary>
public static class CeresFlightArtworkFormat
{
    /// <summary>Required schema revision of the paired front/rear map document.</summary>
    public const int Version = 1;
    /// <summary>Indexed PNG filename for the shared gunship/Ceres chunky Mode-7 characters.</summary>
    public const string FloorFileName = "ceres-flight-mode7-characters.png";
    /// <summary>JSON filename for the two ordered approach map slices, distinct from the later destruction map slice.</summary>
    public const string MapFileName = "ceres-flight-mode7-maps.json";
    /// <summary>Indexed PNG filename for the space/Ceres planar OBJ characters and reused SPACE COLONY background characters.</summary>
    public const string ObjectFileName = "ceres-flight-object-characters.png";
    /// <summary>Mode-7 PNG width in pixels: sixteen 8-pixel character columns.</summary>
    public const int FloorWidth = 128;
    /// <summary>Mode-7 PNG height in pixels: sixteen 8-pixel character rows.</summary>
    public const int FloorHeight = 128;
    /// <summary>Exact chunky Mode-7 character length, $4000 bytes for 256 characters at 64 bytes each.</summary>
    public const int FloorByteCount = 256 * FloorCharacterBytes;
    /// <summary>OBJ PNG width in pixels: thirty-two 8-pixel character columns.</summary>
    public const int ObjectWidth = 256;
    /// <summary>OBJ PNG height in pixels: sixteen 8-pixel character rows.</summary>
    public const int ObjectHeight = 128;
    /// <summary>Exact planar 4-bpp character length, $4000 bytes for 512 characters at 32 bytes each.</summary>
    public const int ObjectByteCount = 512 * ObjectCharacterBytes;
    /// <summary>The chunky 8-bpp pixels of one Mode 7 floor character.</summary>
    public const int FloorCharacterBytes = 64;

    /// <summary>The planar bytes of one 4-bpp OBJ character.</summary>
    public const int ObjectCharacterBytes = 32;
    /// <summary>Number of tile cells per row in each view's host editing-array layout, not the full hardware Mode-7 map stride.</summary>
    public const int MapWidth = 32;
    /// <summary>Number of tile-cell rows in each view's host editing-array layout.</summary>
    public const int MapHeight = 24;
    /// <summary>Number of one-byte tile indices in each front or rear upload slice: 768 ($0300).</summary>
    public const int MapCellsPerView = MapWidth * MapHeight;
    /// <summary>The cells of the front and rear views together, front first.</summary>
    public const int MapCellCount = 2 * MapCellsPerView;
}
