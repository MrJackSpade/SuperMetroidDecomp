
namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>File identity and dimensions for the cartridge's four-row Japanese subtitle staging map.</summary>
public static class IntroFinalLineTilemapFormat
{
    /// <summary>Supported revision of the ordered editable subtitle-map schema.</summary>
    public const int Version = 1;
    /// <summary>Destination cells per BG3 row.</summary>
    public const int Columns = 32;
    /// <summary>$8B:A72B-A82A: two selected subtitle lines, each composed of an upper and lower glyph half.</summary>
    public const int Rows = 2 * 2;
    /// <summary>Required tile-reference count, 128, corresponding to the $0100-byte native map copied into BG3 rows 24-27.</summary>
    public const int CellCount = Columns * Rows;
    /// <summary>Editable JSON filename for the opening cinematic's four-row subtitle staging map, not a prose or animation-sequence file.</summary>
    public const string FileName = "intro-final-text-divider.json";
}
