namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>
/// The cartridge's spelling of the Ceres escape warning pages in glyph tiles (#1466), and the editable document that holds
/// them: where the warning's Latin and Japanese glyph halves sit among the room's characters and the colour variants each
/// page draws with. Extraction composes the pages' pictures from them; gameplay writes the pictures by page.
/// </summary>
internal static class CeresWarningGlyphDefinitions
{
    /// <summary>Supported editable warning-overlay schema revision.</summary>
    internal const int Version = 2;
    /// <summary>Installed JSON file containing all five Ceres warning tilemap pages.</summary>
    internal const string FileName = "ceres-escape-overlay-tilemaps.json";
    /// <summary>Distinct Japanese characters in the native warning font order used to calculate tile selectors.</summary>
    internal const string JapaneseGlyphs = "自爆装置が、作動まただちに脱出して下さい";
    /// <summary>$A6:C4D9: the warning text's characters begin at character $182, where its Latin letters start.</summary>
    internal const int WarningFirstCharacter = 386;
    /// <summary>$A6:C164 selects the Latin atlas beginning with A at the warning's first character.</summary>
    internal const int LatinFirstTile = WarningFirstCharacter;
    /// <summary>$B7:DA00 source loaded at VRAM word $1820; Japanese upper/lower halves start at BG tiles $1A0/$1B0.</summary>
    internal const int JapaneseUpperTile = (JapaneseTrailingUpperTile + MainGlyphCount - 1) / MainGlyphCount * MainGlyphCount,
        JapaneseLowerTile = JapaneseUpperTile + MainGlyphCount;
    /// <summary>$A6:C432..C438 selects the final four upper halves packed before the first sixteen at BG $19C..19F.</summary>
    internal const int JapaneseTrailingUpperTile = LatinFirstTile + ('Z' - 'A' + 1);
    /// <summary>Number of Japanese upper glyph halves stored after the Latin alphabet.</summary>
    internal const int MainGlyphCount = 16;
    /// <summary>The colour variant the English warning uses: the one the self-destruct alarm cycle recolours.</summary>
    internal const int EnglishAlarmPalette = 6;
    /// <summary>The colour variant the Japanese subtitle uses.</summary>
    internal const int JapanesePalette = 7;
}
