namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>The font tiles the cartridge types the opening narration's characters with (#1467).</summary>
internal static class IntroNarrationGlyphTileDefinitions
{
    /// <summary>Tile of uppercase A, from <c>IndirectInstructions_IntroText_A</c> at <c>$8C:D685</c>; A-Z use consecutive tiles in the upper priority tier.</summary>
    internal const int UppercaseATile = 0;
    /// <summary>Tile of digit zero, from the glyph record at <c>$8C:D721</c>; zero through nine use consecutive upper-priority tiles.</summary>
    internal const int DigitZeroTile = 26;
    /// <summary>Blank-space tile from <c>IndirectInstructions_IntroText_Space</c> at <c>$8C:D67D</c>, without the nonblank glyphs' priority.</summary>
    internal const int BlankTile = 47;
    /// <summary>Period tile from <c>IndirectInstructions_IntroText_Period</c> at <c>$8C:D75D</c>, distinct from the unused decimal-point glyph.</summary>
    internal const int PeriodTile = 36;
    /// <summary>Comma tile from <c>IndirectInstructions_IntroText_Comma</c> at <c>$8C:D763</c>.</summary>
    internal const int CommaTile = 37;
    /// <summary>Apostrophe tile from <c>IndirectInstructions_IntroText_Apostrophe</c> at <c>$8C:D76F</c>.</summary>
    internal const int ApostropheTile = 39;
    /// <summary>Exclamation tile from <c>IndirectInstructions_IntroText_ExclamationPoint</c> at <c>$8C:D77B</c>.</summary>
    internal const int ExclamationTile = 42;
}
