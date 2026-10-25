using System.Collections.Immutable;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Native identity and queue geometry for the English and Japanese Ceres overlays.</summary>
internal static class CeresEscapeOverlayTilemapDefinitions
{
    /// <summary>Supported editable warning-overlay schema revision.</summary>
    internal const int Version = 1;
    /// <summary>Installed JSON file containing all five Ceres warning tilemap pages.</summary>
    internal const string FileName = "ceres-escape-overlay-tilemaps.json";

    /// <summary>English EMERGENCY title tilemap at $A6:C164, written at BG1 row six, column eleven.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition Emergency =
        new("emergency", 0xa6c164, EmergencyText.Length, 6 * RowCells + 11);

    /// <summary>First Japanese self-destruct subtitle row at $A6:C3F4, written at BG1 row twenty, column ten.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition JapaneseFirst =
        new("japanese_0", 0xa6c3f4, JapaneseFirstLine.Length, 20 * RowCells + 10);

    /// <summary>Second Japanese self-destruct subtitle row at $A6:C40C, the row below the first.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition JapaneseSecond =
        new("japanese_1", JapaneseFirst.SourceAddress + JapaneseFirst.WordCount * sizeof(ushort), JapaneseFirstLine.Length,
            JapaneseFirst.Cell + RowCells);

    /// <summary>Third Japanese self-destruct subtitle row at $A6:C424, the row below the second.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition JapaneseThird =
        new("japanese_2", JapaneseSecond.SourceAddress + JapaneseSecond.WordCount * sizeof(ushort), JapaneseSecondLine.Length,
            JapaneseSecond.Cell + RowCells);

    /// <summary>Fourth Japanese self-destruct subtitle row at $A6:C43A, the row below the third.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition JapaneseFourth =
        new("japanese_3", JapaneseThird.SourceAddress + JapaneseThird.WordCount * sizeof(ushort), JapaneseSecondLine.Length,
            JapaneseThird.Cell + RowCells);

    /// <summary>English warning title rendered from the Latin tile alphabet.</summary>
    /// <remarks>Authored warning wording and font ordering are categorical typography content. The selected two-half
    /// packing, foreground priority and Japanese palette role preserve that composition; glyph pixels and RGB paint
    /// remain separate obligations.</remarks>
    private const string EmergencyText = "EMERGENCY";
    /// <summary>First authored Japanese self-destruct subtitle row, shared by its upper and lower tile halves.</summary>
    private const string JapaneseFirstLine = "自爆装置が、作動しました";
    /// <summary>Second authored Japanese evacuation subtitle row, shared by its upper and lower tile halves.</summary>
    private const string JapaneseSecondLine = "ただちに脱出して下さい";
    /// <summary>Distinct Japanese characters in the native warning font order used to calculate tile selectors.</summary>
    private const string JapaneseGlyphs = "自爆装置が、作動まただちに脱出して下さい";
    /// <summary>The cells in one overlay row.</summary>
    private const int RowCells = 32;
    /// <summary>$A6:C4D9: the warning text's characters begin at character $182, where its Latin letters start.</summary>
    internal const int WarningFirstCharacter = 0x182;
    /// <summary>$A6:C164 selects the Latin atlas beginning with A at the warning's first character.</summary>
    private const int LatinFirstTile = WarningFirstCharacter;
    /// <summary>$B7:DA00 source loaded at VRAM word $1820; Japanese upper/lower halves start at BG tiles $1A0/$1B0.</summary>
    private const int JapaneseUpperTile = (JapaneseTrailingUpperTile + MainGlyphCount - 1) / MainGlyphCount * MainGlyphCount,
        JapaneseLowerTile = JapaneseUpperTile + MainGlyphCount;
    /// <summary>$A6:C432..C438 selects the final four upper halves packed before the first sixteen at BG $19C..19F.</summary>
    private const int JapaneseTrailingUpperTile = LatinFirstTile + ('Z' - 'A' + 1);
    /// <summary>Number of Japanese upper glyph halves stored after the Latin alphabet.</summary>
    private const int MainGlyphCount = 16;
    /// <summary>BG tile attribute selecting the foreground warning layer.</summary>
    private const int Priority = 1 << 13;
    /// <summary>The background palette the English warning uses: the one the self-destruct alarm cycle recolours.</summary>
    private const int EnglishAlarmPalette = 6;
    /// <summary>Foreground priority combined with the English alarm and Japanese subtitle palette rows.</summary>
    private const int EnglishStyle = Priority | EnglishAlarmPalette << 10,
        JapaneseStyle = Priority | 7 << 10;

    /// <summary>Calculates a stock English or Japanese overlay word from its authored glyph order, tile half, priority, and palette.</summary>
    internal static ushort StockWord(CeresEscapeOverlayTilemapDefinition page, int index)
    {
        if ((uint)index >= page.WordCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (page == Emergency) return (ushort)(EnglishStyle | LatinFirstTile + EmergencyText[index] - 'A');
        bool secondLine = page == JapaneseThird || page == JapaneseFourth;
        bool lowerHalf = page == JapaneseSecond || page == JapaneseFourth;
        if (page != JapaneseFirst && page != JapaneseSecond && page != JapaneseThird && page != JapaneseFourth)
            throw new ArgumentOutOfRangeException(nameof(page));
        char character = (secondLine ? JapaneseSecondLine : JapaneseFirstLine)[index];
        int glyph = JapaneseGlyphs.IndexOf(character, StringComparison.Ordinal);
        if (glyph < 0) throw new InvalidOperationException("Ceres subtitle character has no native glyph.");
        int tile = lowerHalf ? JapaneseLowerTile + glyph : glyph < MainGlyphCount
            ? JapaneseUpperTile + glyph : JapaneseTrailingUpperTile + glyph - MainGlyphCount;
        return (ushort)(JapaneseStyle | tile);
    }
    /// <summary>The five fixed overlays in transfer order: the English title, then the four Japanese row halves.</summary>
    internal static ImmutableArray<CeresEscapeOverlayTilemapDefinition> All { get; } =
        [Emergency, JapaneseFirst, JapaneseSecond, JapaneseThird, JapaneseFourth];
    /// <summary>The Japanese subtitle's four row halves, in the order the one-shot list at $A6:C3B8 writes them.</summary>
    internal static ImmutableArray<CeresEscapeOverlayTilemapDefinition> Japanese { get; } =
        [JapaneseFirst, JapaneseSecond, JapaneseThird, JapaneseFourth];
}
