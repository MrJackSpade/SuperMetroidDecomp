using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using static SuperMetroid.Core.Areas.Ceres.Artwork.CeresEscapeOverlayTilemapDefinitions;
using static SuperMetroid.AssetExtraction.Areas.Ceres.CeresWarningGlyphDefinitions;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>
/// Spells the Ceres escape warnings in their glyph tiles (<see cref="CeresWarningGlyphDefinitions"/>): the English title in the
/// alarm colour variant, and each Japanese row half from its glyph order and tile half.
/// </summary>
internal static class CeresEscapeWarningGlyphs
{
    /// <summary>
    /// Calculates a stock English or Japanese overlay tile from its authored glyph order and tile half, in its colour
    /// variant and the warning's upper priority tier.
    /// </summary>
    internal static BackgroundTile StockTile(CeresEscapeOverlayTilemapDefinition page, int index)
    {
        if ((uint)index >= page.WordCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (page == Emergency) return new BackgroundTile(LatinFirstTile + EmergencyText[index] - 'A', EnglishAlarmPalette, highPriority: true);
        bool secondLine = page == JapaneseThird || page == JapaneseFourth;
        bool lowerHalf = page == JapaneseSecond || page == JapaneseFourth;
        if (page != JapaneseFirst && page != JapaneseSecond && page != JapaneseThird && page != JapaneseFourth)
            throw new ArgumentOutOfRangeException(nameof(page));
        char character = (secondLine ? JapaneseSecondLine : JapaneseFirstLine)[index];
        int glyph = JapaneseGlyphs.IndexOf(character, StringComparison.Ordinal);
        if (glyph < 0) throw new InvalidOperationException("Ceres subtitle character has no native glyph.");
        int tile = lowerHalf ? JapaneseLowerTile + glyph : glyph < MainGlyphCount
            ? JapaneseUpperTile + glyph : JapaneseTrailingUpperTile + glyph - MainGlyphCount;
        return new BackgroundTile(tile, JapanesePalette, highPriority: true);
    }
}
