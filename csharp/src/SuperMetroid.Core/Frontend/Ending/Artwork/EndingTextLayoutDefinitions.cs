using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Calculated post-credit text placement and ending-font atlas traversal.</summary>
internal static class EndingTextLayoutDefinitions
{
    /// <summary>$8C:DF6D starts the top row of アイテム発見パーセンテージ at ending-font tile$5B.</summary>
    private const int SubtitleFirstTopTile = 0x5b;
    /// <summary>$8C:DF6D..DF85 has thirteen consecutive phrase glyphs, centered in the32-cell screen row.</summary>
    private const int SubtitleGlyphCount = 13;
    /// <summary>$8C:DF77 continues the phrase at tile$80, after the two atlas rows containing copyright digits ($60..$79).</summary>
    private const int SubtitleContinuationTopTile = EndingTextDefinitions.CopyrightDigitTopBase +
        EndingTextDefinitions.JapaneseSubtitleRows * EndingFontAtlasFormat.TilesPerRow;
    /// <summary>Two complete tilemap rows cover the Japanese subtitle's upper and lower glyph halves.</summary>
    internal const int SubtitleCellCount = EndingTextDefinitions.JapaneseSubtitleRows * EndingTextDefinitions.TilemapWidth;

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
    /// <summary>$8C:DD31 glyphs spell the chosen credit identity DEER FORCE; narrowly retained lexical content.</summary>
    /// <remarks>Exact stock credit wording is retained as chosen lexical content: the native consumer copies glyphs
    /// opaquely; generating the spelling from gameplay is nonsense. This narrow disposition excludes the independently
    /// required palette choices and font pixels.</remarks>
    private const string DevelopmentGroup = "DEER FORCE";
    /// <summary>$8C:DDF9 glyphs spell OF, the chosen connective wording of the credit; narrowly retained lexical content.</summary>
    private const string AttributionPreposition = "OF";
    /// <summary>$8C:DE6D glyphs spell the chosen credit identity TEAM SHIKAMARU; narrowly retained lexical content.</summary>
    private const string DevelopmentTeam = "TEAM SHIKAMARU";
    /// <summary>$8C:DC9B..DEDA fills nine rows with two small and two large lines, distributing the remaining rows equally across the three intervening gaps.</summary>
    private const int CreditLineGap = (EndingTextDefinitions.ResultPanelRows -
        2 * (EndingTextDefinitions.SmallGlyphHeight + EndingTextDefinitions.LargeGlyphHeight)) / 3;
    /// <summary>$8C:DD31 shows the DEER FORCE label in colour variant 6; this chosen style remains required.</summary>
    private const int DevelopmentGroupColourVariant = 6;
    /// <summary>$8C:DE6D shows TEAM SHIKAMARU in colour variant 7; this chosen style remains required.</summary>
    private const int DevelopmentTeamColourVariant = 7;
    /// <summary>The complete result panel spans every authored row at the native tilemap width.</summary>
    internal const int ResultCellCount = EndingTextDefinitions.ResultPanelRows * EndingTextDefinitions.TilemapWidth;

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
        return EndingTextDefinitions.CompileGlyph(text[character], style, bottom) with { ColourVariant = colourVariant };
    }
}
