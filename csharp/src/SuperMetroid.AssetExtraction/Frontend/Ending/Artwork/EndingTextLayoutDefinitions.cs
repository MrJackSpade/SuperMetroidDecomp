namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Calculated post-credit text placement and ending-font atlas traversal.</summary>
internal static class EndingTextLayoutDefinitions
{
    /// <summary>$8C:DF6D starts the top row of アイテム発見パーセンテージ at ending-font tile$5B.</summary>
    internal const int SubtitleFirstTopTile = 0x5b;
    /// <summary>$8C:DF6D..DF85 has thirteen consecutive phrase glyphs, centered in the32-cell screen row.</summary>
    internal const int SubtitleGlyphCount = 13;
    /// <summary>$8C:DF77 continues the phrase at tile$80, after the two atlas rows containing copyright digits ($60..$79).</summary>
    internal const int SubtitleContinuationTopTile = EndingTextDefinitions.CopyrightDigitTopBase +
        EndingTextDefinitions.JapaneseSubtitleRows * EndingFontAtlasFormat.TilesPerRow;
    /// <summary>Two complete tilemap rows cover the Japanese subtitle's upper and lower glyph halves.</summary>
    internal const int SubtitleCellCount = EndingTextDefinitions.JapaneseSubtitleRows * EndingTextDefinitions.TilemapWidth;
    /// <summary>$8C:DD31 glyphs spell the chosen credit identity DEER FORCE; narrowly retained lexical content.</summary>
    /// <remarks>Exact stock credit wording is retained as chosen lexical content: the native consumer copies glyphs
    /// opaquely; generating the spelling from gameplay is nonsense. This narrow disposition excludes the independently
    /// required palette choices and font pixels.</remarks>
    internal const string DevelopmentGroup = "DEER FORCE";
    /// <summary>$8C:DDF9 glyphs spell OF, the chosen connective wording of the credit; narrowly retained lexical content.</summary>
    internal const string AttributionPreposition = "OF";
    /// <summary>$8C:DE6D glyphs spell the chosen credit identity TEAM SHIKAMARU; narrowly retained lexical content.</summary>
    internal const string DevelopmentTeam = "TEAM SHIKAMARU";
    /// <summary>$8C:DC9B..DEDA fills nine rows with two small and two large lines, distributing the remaining rows equally across the three intervening gaps.</summary>
    internal const int CreditLineGap = (EndingTextDefinitions.ResultPanelRows -
        2 * (EndingTextDefinitions.SmallGlyphHeight + EndingTextDefinitions.LargeGlyphHeight)) / 3;
    /// <summary>$8C:DD31 shows the DEER FORCE label in colour variant 6; this chosen style remains required.</summary>
    internal const int DevelopmentGroupColourVariant = 6;
    /// <summary>$8C:DE6D shows TEAM SHIKAMARU in colour variant 7; this chosen style remains required.</summary>
    internal const int DevelopmentTeamColourVariant = 7;
    /// <summary>The complete result panel spans every authored row at the native tilemap width.</summary>
    internal const int ResultCellCount = EndingTextDefinitions.ResultPanelRows * EndingTextDefinitions.TilemapWidth;
}
