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
    /// the descriptors and the drawing window are import data (#1638).
    /// </summary>
    public const int Version = 7;
    /// <summary>Installed layout JSON filename: each pose's typed cover placement, which the game binds (#1638).</summary>
    public const string JsonFileName = "samus-arm-cannon.json";
    /// <summary>
    /// Import-data filename of the extracted document (#1638): the layout with the cover appearances and tile selections the
    /// overlay sheet is composed from.
    /// </summary>
    public const string ArtworkFileName = "samus-arm-cannon-artwork.json";
    /// <summary>Installed indexed PNG filename holding the twelve 8-by-8 cover characters in native source-identity order.</summary>
    public const string TileFileName = "samus-arm-cannon-tiles.png";
    /// <summary>The 608 extracted bytes of the drawing window: the pose descriptors, then the cost table they over-read.</summary>
    public const int DrawingDataByteCount = 608;
    /// <summary>Four tile-selector slots per native aim direction: closed sentinel 0 and three successive nonclosed cover-art frames.</summary>
    public const int FramesPerDirection = 4;
    /// <summary>Installed cover tiles: three opening frames for each of the four artwork orientations.</summary>
    public const int CoverTileCount = 12;
    /// <summary>Bytes of one installed cover tile: a single 4bpp 8x8 character.</summary>
    public const int CoverTileByteCount = 32;
}
