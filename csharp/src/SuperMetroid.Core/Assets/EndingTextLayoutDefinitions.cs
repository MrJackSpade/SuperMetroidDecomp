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
    /// <summary>Number of tilemap cells traversed by the Japanese subtitle layout.</summary>
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
    // Exact stock credit wording is retained as chosen lexical content: the native
    // consumer copies glyphs opaquely; generating the spelling from gameplay is nonsense.
    // This narrow disposition excludes the independently required palette choices and font pixels.
    /// <summary>$8C:DD31 glyphs spell the chosen credit identity DEER FORCE; narrowly retained lexical content.</summary>
    private const string DevelopmentGroup = "DEER FORCE";
    /// <summary>$8C:DDF9 glyphs spell OF, the chosen connective wording of the credit; narrowly retained lexical content.</summary>
    private const string AttributionPreposition = "OF";
    /// <summary>$8C:DE6D glyphs spell the chosen credit identity TEAM SHIKAMARU; narrowly retained lexical content.</summary>
    private const string DevelopmentTeam = "TEAM SHIKAMARU";
    /// <summary>$8C:DC9B..DEDA fills nine rows with two small and two large lines, distributing the remaining rows equally across the three intervening gaps.</summary>
    private const int CreditLineGap = (EndingTextDefinitions.ResultPanelRows -
        2 * (EndingTextDefinitions.Native.SmallGlyphHeight + EndingTextDefinitions.Native.LargeGlyphHeight)) / 3;
    /// <summary>$8C:DD31 applies BG palette6 to the DEER FORCE label; this chosen style remains required.</summary>
    private const ushort DevelopmentGroupAttributes = 6 << 10;
    /// <summary>$8C:DE6D applies BG palette7 to TEAM SHIKAMARU; this chosen style remains required.</summary>
    private const ushort DevelopmentTeamAttributes = 7 << 10;
    /// <summary>Number of tilemap cells in the ending result panel.</summary>
    internal const int ResultCellCount = EndingTextDefinitions.ResultPanelRows * EndingTextDefinitions.TilemapWidth;

    /// <summary>
    /// Center the four credit lines, distribute available blank rows between their actual
    /// glyph heights, and generate large lower halves from the font layout. The supplied
    /// heading defaults to $8C:DCAF PRODUCED BY, the fourth narrowly retained stock phrase.
    /// </summary>
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

    /// <summary>
    /// Places the supplied text horizontally within the tilemap row and returns the glyph at the requested cell.
    /// </summary>
    /// <param name="text">Text whose glyph sequence is centered.</param>
    /// <param name="column">Tilemap column to resolve.</param>
    /// <param name="large">Whether to use the large copyright font.</param>
    /// <param name="bottom">Whether the requested glyph is the lower half of a large character.</param>
    /// <param name="attributes">Tilemap attributes to combine with a nonblank glyph.</param>
    /// <returns>The compiled tile word for the centered glyph, or the blank word outside the text.</returns>
    private static ushort Centered(string text, int column, bool large, bool bottom, ushort attributes) =>
        Glyph(text, column - (EndingTextDefinitions.TilemapWidth - text.Length) / 2, large, bottom, attributes);

    /// <summary>
    /// Resolves one character to its font tile word, returning the blank word for spaces and out-of-range positions.
    /// </summary>
    /// <param name="text">Text supplying the character.</param>
    /// <param name="character">Zero-based character position.</param>
    /// <param name="large">Whether the large copyright font is used.</param>
    /// <param name="bottom">Whether to select the lower tile of a large glyph.</param>
    /// <param name="attributes">Tilemap attributes combined with the compiled glyph word.</param>
    /// <returns>The compiled character tile word or the blank word.</returns>
    private static ushort Glyph(string text, int character, bool large, bool bottom, ushort attributes)
    {
        if ((uint)character >= text.Length || text[character] == ' ')
            return EndingTextDefinitions.ResultBlankWord;
        return (ushort)(attributes | EndingTextDefinitions.CompileGlyph(text[character],
            large ? EndingTextStyle.CopyrightLarge : EndingTextStyle.ResultSmall, bottom));
    }
}
