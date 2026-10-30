using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>File identity and dimensions for the cartridge's four-row Japanese subtitle staging map.</summary>
public static class IntroFinalLineTilemapFormat
{
    /// <summary>Supported revision of the ordered editable subtitle-map schema.</summary>
    public const int Version = 1;
    /// <summary>Destination cells per BG3 row, comprising 24 stock subtitle columns and four stock margin columns on each side.</summary>
    public const int Columns = 32;
    /// <summary>$8B:A72B-A82A: two selected subtitle lines, each composed of an upper and lower glyph half.</summary>
    public const int Rows = 2 * 2;
    /// <summary>Required tile-reference count, 128, corresponding to the $0100-byte native map copied into BG3 rows 24-27.</summary>
    public const int CellCount = Columns * Rows;
    /// <summary>$8B:8DEA/A68F: 0600 bytes of 2bpp glyph staging form four displayed tile rows, yielding 24 columns.</summary>
    internal const int InteriorColumns = IntroCinematicDefinitions.Artwork.JapaneseBlankCharactersByteCount /
        (IntroFontAtlasFormat.BitsPerPixel * IntroFontAtlasFormat.TileSize * Rows);
    /// <summary>$8B:A72B-A82A: center the subtitle staging width in the 32-column BG page.</summary>
    internal const int MarginColumns = (Columns - InteriorColumns) / 2;
    /// <summary>$8B:A6B7: margins share the surrounding Japanese blank tile, colour variant and priority: the blank cell of the Japanese narration layout, in front.</summary>
    internal static BackgroundTile MarginTile => new(IntroFinalLineTilemapDefinitions.JapaneseBlankGlyph,
        IntroFinalLineTilemapDefinitions.JapaneseBlankColourVariant, highPriority: true);
    /// <summary>$8B:A72B: first subtitle tile follows its VRAM staging destination relative to the font base; colour variant four and priority are selected subtitle style.</summary>
    internal static BackgroundTile FirstInteriorTile => new(
        IntroTileSetDefinitions.JapaneseBlankFirstTile, SubtitlePalette, highPriority: true);
    /// <summary>$8B:A733: chosen palette four for the Japanese subtitle glyph staging area; this display policy is retained as selected typesetting, not prose, pixels or timing.</summary>
    private const int SubtitlePalette = 4;
    /// <summary>Editable JSON filename for the opening cinematic's four-row subtitle staging map, not a prose or animation-sequence file.</summary>
    public const string FileName = "intro-final-text-divider.json";
}
