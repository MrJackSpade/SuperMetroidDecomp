namespace SuperMetroid.Core.Assets;

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
    internal const int SubtitleCellCount = EndingTextDefinitions.JapaneseSubtitleRows * EndingTextDefinitions.TilemapWidth;

    /// <summary>
    /// $8C:DF5B..DFDA: center the phrase, consume the partial atlas row, then
    /// continue after the copyright digit pair and select each lower half. Chosen phrase
    /// pixels remain independent required content in the ending-font artwork.
    /// </summary>
    internal static ushort SubtitleWord(int cell)
    {
        if ((uint)cell >= SubtitleCellCount) throw new IndexOutOfRangeException();
        int row = cell / EndingTextDefinitions.TilemapWidth;
        int column = cell % EndingTextDefinitions.TilemapWidth;
        int glyph = column - (EndingTextDefinitions.TilemapWidth - SubtitleGlyphCount) / 2;
        if ((uint)glyph >= SubtitleGlyphCount) return EndingTextDefinitions.ResultBlankWord;
        int atlasWidth = EndingFontAtlasFormat.TilesPerRow;
        int firstRunCount = atlasWidth - SubtitleFirstTopTile % atlasWidth;
        int topTile = glyph < firstRunCount ? SubtitleFirstTopTile + glyph
            : SubtitleContinuationTopTile + glyph - firstRunCount;
        return (ushort)(topTile + row * atlasWidth);
    }
    // These chosen lexical and styling inputs remain required under resultPanel.
    // No authorship or visual preference exemption is claimed for them.
    private const string DevelopmentGroup = "DEER FORCE";
    private const string AttributionPreposition = "OF";
    private const string DevelopmentTeam = "TEAM SHIKAMARU";
    /// <summary>$8C:DC9B panel separates its four text lines with one blank tile row; this chosen spacing remains required.</summary>
    private const int CreditLineGap = 1;
    /// <summary>$8C:DD31 applies BG palette6 to the DEER FORCE label; this chosen style remains required.</summary>
    private const ushort DevelopmentGroupAttributes = 6 << 10;
    /// <summary>$8C:DE6D applies BG palette7 to TEAM SHIKAMARU; this chosen style remains required.</summary>
    private const ushort DevelopmentTeamAttributes = 7 << 10;
    internal const int ResultCellCount = EndingTextDefinitions.ResultPanelRows * EndingTextDefinitions.TilemapWidth;

    /// <summary>Center the four credit lines, stack their actual glyph heights with a blank row between lines, and generate large lower halves from the shared font layout.</summary>
    internal static ushort ResultWord(int cell, string producedBy)
    {
        if ((uint)cell >= ResultCellCount) throw new IndexOutOfRangeException();
        int row = cell / EndingTextDefinitions.TilemapWidth;
        int column = cell % EndingTextDefinitions.TilemapWidth;
        int groupRow = EndingTextDefinitions.Native.SmallGlyphHeight + CreditLineGap;
        int prepositionRow = groupRow + EndingTextDefinitions.Native.LargeGlyphHeight + CreditLineGap;
        int teamRow = prepositionRow + EndingTextDefinitions.Native.SmallGlyphHeight + CreditLineGap;
        if (row == EndingTextDefinitions.ResultProducedBy.Row)
            return Glyph(producedBy, column - EndingTextDefinitions.ResultProducedBy.Column, large: false, bottom: false, attributes: 0);
        if (row >= groupRow && row < groupRow + EndingTextDefinitions.Native.LargeGlyphHeight)
            return Centered(DevelopmentGroup, column, large: true, row != groupRow, DevelopmentGroupAttributes);
        if (row == prepositionRow)
            return Centered(AttributionPreposition, column, large: false, bottom: false, attributes: 0);
        if (row >= teamRow && row < teamRow + EndingTextDefinitions.Native.LargeGlyphHeight)
            return Centered(DevelopmentTeam, column, large: true, row != teamRow, DevelopmentTeamAttributes);
        return EndingTextDefinitions.ResultBlankWord;
    }

    private static ushort Centered(string text, int column, bool large, bool bottom, ushort attributes) =>
        Glyph(text, column - (EndingTextDefinitions.TilemapWidth - text.Length) / 2, large, bottom, attributes);

    private static ushort Glyph(string text, int character, bool large, bool bottom, ushort attributes)
    {
        if ((uint)character >= text.Length || text[character] == ' ')
            return EndingTextDefinitions.ResultBlankWord;
        return (ushort)(attributes | EndingTextDefinitions.CompileGlyph(text[character],
            large ? EndingTextStyle.CopyrightLarge : EndingTextStyle.ResultSmall, bottom));
    }
}
