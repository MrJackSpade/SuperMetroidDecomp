namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>The installed arm-cannon artwork files and their fixed table dimensions, separate from open/close mechanics.</summary>
public static class SamusArmCannonArtworkFormat
{
    /// <summary>
    /// Supported placement JSON schema revision, checked with the fixed pose, descriptor, direction, and frame table
    /// dimensions. Revision four keys each pose's descriptor by the pose's name rather than its place in the cartridge's
    /// pose table, and installs no descriptor for the records no pose names (#1595). Revision six installs only the layout
    /// the game binds: the cover appearances, whose entries are packed sprite attributes, and the tile selections are
    /// import data (#1638). Revision seven installs each pose's typed placement instead of the cartridge's descriptor bytes:
    /// its drawing order, the aim directions it shows and the cover's displacement at each animation frame the pose shows;
    /// the descriptors and the drawing window are import data (#1638). Revision eight installs a displacement at every
    /// animation frame each pose shows: the drawing window reaches the last displacement byte a shown frame reads (#1638).
    /// </summary>
    public const int Version = 8;
    /// <summary>Installed layout JSON filename: each pose's typed cover placement, which the game binds (#1638).</summary>
    public const string JsonFileName = "samus-arm-cannon.json";
    /// <summary>
    /// Import-data filename of the extracted document (#1638): the layout with the cover appearances and tile selections the
    /// overlay sheet is composed from.
    /// </summary>
    public const string ArtworkFileName = "samus-arm-cannon-artwork.json";
    /// <summary>Installed indexed PNG filename holding the twelve 8-by-8 cover characters in native source-identity order.</summary>
    public const string TileFileName = "samus-arm-cannon-tiles.png";
    /// <summary>
    /// The 656 extracted bytes of the drawing window, $90:C9D9 through $CC68 (#1638): the pose descriptors, then the
    /// special-attack cost table and the first bytes of the routine after it, which the cover's draw reads as displacements
    /// at the later animation frames the up-aiming transitions and landings show. The last byte is the vertical displacement
    /// of the left-facing aim-up transition descriptor at animation frame 37, the farthest any pose shows; the import fails
    /// rather than install a shown frame whose displacement lies past the window.
    /// </summary>
    public const int DrawingDataByteCount = 656;
    /// <summary>Four tile-selector slots per native aim direction: closed sentinel 0 and three successive nonclosed cover-art frames.</summary>
    public const int FramesPerDirection = 4;
    /// <summary>Installed cover tiles: three opening frames for each of the four artwork orientations.</summary>
    public const int CoverTileCount = 12;
    /// <summary>Bytes of one installed cover tile: a single 4bpp 8x8 character.</summary>
    public const int CoverTileByteCount = 32;
}
