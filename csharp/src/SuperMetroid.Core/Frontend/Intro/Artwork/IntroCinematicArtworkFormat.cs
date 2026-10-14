namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>File identities and physical 4-bpp transfer lengths for the opening scene.</summary>
public static class IntroCinematicArtworkFormat
{
    /// <summary>Opening bundle's source-identity and file-hash manifest resource, <c>intro-artwork.json</c>.</summary>
    public const string ManifestFileName = "intro-artwork.json";
    /// <summary>Palette-indexed PNG sheet of opening BG characters, compiled into <see cref="BackgroundByteCount"/> bytes of 4-bpp tile data.</summary>
    public const string BackgroundFileName = "intro-background-characters.png";
    /// <summary>Palette-indexed PNG sheet of fixed-bank intro OBJ characters, compiled into <see cref="IntroObjectByteCount"/> bytes.</summary>
    public const string IntroObjectFileName = "intro-object-characters.png";
    /// <summary>Palette-indexed PNG sheet of cinematic OBJ characters, compiled into <see cref="CinematicObjectByteCount"/> bytes.</summary>
    public const string CinematicObjectFileName = "intro-cinematic-object-characters.png";
    /// <summary>Editable JSON tile references for the single Samus-head portrait BG page.</summary>
    public const string PortraitTilemapFileName = "intro-portrait-tilemap.json";
    /// <summary>Editable JSON tile references for the single initial narration BG3 page, before incremental typewriter updates.</summary>
    public const string InitialNarrationTilemapFileName = "intro-initial-narration-tilemap.json";
    /// <summary>Four ordered BG tilemap pages in the opening illustrated-page transfer.</summary>
    public const int BackgroundPageCount = 4;
    /// <summary>$0800 transfer bytes per 32x32 BG page: 1024 little-endian, two-byte tile-reference words.</summary>
    public const int BackgroundPageByteCount = 1024 * sizeof(ushort);
    /// <summary>Formats the installed JSON resource name for one ordered background page without validating its index.</summary>
    /// <param name="index">Zero-based page index, normally 0 through 3.</param>
    /// <returns>The resource name <c>intro-background-page-{index}.json</c>.</returns>
    public static string BackgroundPageFileName(int index) => $"intro-background-page-{index}.json";
    /// <summary>$8000 bytes of 4-bpp BG data: 1024 eight-by-eight characters uploaded at VRAM byte $0000.</summary>
    public const int BackgroundByteCount = 1024 * 32;
    /// <summary>$2000 bytes of fixed-bank 4-bpp intro OBJ data: 256 characters uploaded at VRAM byte $C000.</summary>
    public const int IntroObjectByteCount = 256 * 32;
    /// <summary>$2400 bytes of 4-bpp cinematic OBJ data: 288 characters uploaded at VRAM byte $DC00.</summary>
    public const int CinematicObjectByteCount = 288 * 32;
    /// <summary>Thirty-two eight-pixel character cells per PNG-sheet row; also the tile-column stride of a 32x32 BG page.</summary>
    public const int TileColumns = 32;
}
