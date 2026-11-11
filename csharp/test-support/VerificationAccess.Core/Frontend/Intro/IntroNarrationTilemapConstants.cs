using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The cartridge's narration tilemap words that checks rebuild the narration plane's memory image from (#1467): the blank
/// cells the staging map is cleared to and its border's layout. The cinematic shows the narration as display-model text
/// and keeps none of them.
/// </summary>
internal static class IntroNarrationTilemapConstants
{
    /// <summary>The first narration tilemap cell the cartridge clears for the gameplay text, the cells before it holding the border.</summary>
    internal const int GameplayBlankStartIndex = 128;

    /// <summary>The first narration tilemap cell the installed final text line replaces.</summary>
    internal const int FinalLineFirstCell = 768;

    /// <summary>The blank cell written over the gameplay text region, the tile a typed space shows.</summary>
    internal static BackgroundTile Blank { get; } = new(0x02f, 0, highPriority: false);

    /// <summary>The blank cell around the final text line: the blank glyph, colour variant seven.</summary>
    internal static BackgroundTile FinalBlank { get; } = new(0x029, 7, highPriority: false);

    /// <summary>
    /// $8B:A6B7: the staging map's border and margin cell, the Japanese narration layout's blank glyph in colour variant
    /// seven, in front.
    /// </summary>
    internal static BackgroundTile Margin { get; } = new(0x029, 7, highPriority: true);

    /// <summary>Cells from the upper to the lower Japanese blank region.</summary>
    internal const int JapaneseBlankBottomDelta = 896;

    /// <summary>The cell blanked immediately left of the final text, which `menu.menu_tilemap`'s offset $11E aliases.</summary>
    internal const int FinalBlankLeftIndex = 911;

    /// <summary>The cell blanked immediately right of the final text.</summary>
    internal const int FinalBlankRightIndex = 912;
}
