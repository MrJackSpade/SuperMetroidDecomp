using static SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingTextLayoutDefinitions;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>
/// Lays out the ending's text panels (<see cref="EndingTextLayoutDefinitions"/>): the Japanese subtitle and the result panel's
/// centered credit lines.
/// </summary>
internal static class EndingTextPanels
{
    /// <summary>
    /// $8C:DF5B..DFDA: center the phrase, consume the partial atlas row, then
    /// continue after the copyright digit pair and select each lower half. Chosen phrase
    /// pixels remain independent required content in the ending-font artwork.
    /// </summary>
    internal static BackgroundTile SubtitleTile(int cell)
    {
        if ((uint)cell >= SubtitleCellCount) throw new ArgumentOutOfRangeException(nameof(cell));
        int row = cell / EndingTextDefinitions.TilemapWidth;
        int column = cell % EndingTextDefinitions.TilemapWidth;
        int glyph = column - (EndingTextDefinitions.TilemapWidth - SubtitleGlyphCount) / 2;
        if ((uint)glyph >= SubtitleGlyphCount) return EndingTextDefinitions.ResultBlankTile;
        int atlasWidth = EndingFontAtlasFormat.TilesPerRow;
        int firstRunCount = atlasWidth - SubtitleFirstTopTile % atlasWidth;
        int topTile = glyph < firstRunCount ? SubtitleFirstTopTile + glyph
            : SubtitleContinuationTopTile + glyph - firstRunCount;
        return new(topTile + row * atlasWidth, 0, highPriority: false);
    }

    /// <summary>
    /// Center the four credit lines, distribute available blank rows between their actual
    /// glyph heights, and generate large lower halves from the font layout. The supplied
    /// heading defaults to $8C:DCAF PRODUCED BY, the fourth narrowly retained stock phrase.
    /// </summary>
    internal static BackgroundTile ResultTile(int cell, string producedBy)
    {
        if ((uint)cell >= ResultCellCount) throw new ArgumentOutOfRangeException(nameof(cell));
        int row = cell / EndingTextDefinitions.TilemapWidth;
        int column = cell % EndingTextDefinitions.TilemapWidth;
        int groupRow = EndingTextDefinitions.SmallGlyphHeight + CreditLineGap;
        int prepositionRow = groupRow + EndingTextDefinitions.LargeGlyphHeight + CreditLineGap;
        int teamRow = prepositionRow + EndingTextDefinitions.SmallGlyphHeight + CreditLineGap;
        if (row == EndingTextDefinitions.ResultProducedBy.Row)
            return Glyph(producedBy, column - EndingTextDefinitions.ResultProducedBy.Column, EndingTextStyle.ResultSmall, bottom: false, colourVariant: 0);
        if (row >= groupRow && row < groupRow + EndingTextDefinitions.LargeGlyphHeight)
            return Centered(DevelopmentGroup, column, EndingTextStyle.CopyrightLarge, row != groupRow, DevelopmentGroupColourVariant);
        if (row == prepositionRow)
            return Centered(AttributionPreposition, column, EndingTextStyle.ResultSmall, bottom: false, colourVariant: 0);
        if (row >= teamRow && row < teamRow + EndingTextDefinitions.LargeGlyphHeight)
            return Centered(DevelopmentTeam, column, EndingTextStyle.CopyrightLarge, row != teamRow, DevelopmentTeamColourVariant);
        return EndingTextDefinitions.ResultBlankTile;
    }

    /// <summary>Centers a selected credit phrase before resolving its top or bottom glyph tile.</summary>
    private static BackgroundTile Centered(string text, int column, EndingTextStyle style, bool bottom, int colourVariant) =>
        Glyph(text, column - (EndingTextDefinitions.TilemapWidth - text.Length) / 2, style, bottom, colourVariant);

    /// <summary>Leaves spaces blank and shows a compiled ending-font glyph in its selected colour variant.</summary>
    private static BackgroundTile Glyph(string text, int character, EndingTextStyle style, bool bottom, int colourVariant)
    {
        if ((uint)character >= text.Length || text[character] == ' ')
            return EndingTextDefinitions.ResultBlankTile;
        return EndingTextGlyphs.CompileGlyph(text[character], style, bottom) with { ColourVariant = colourVariant };
    }
}
