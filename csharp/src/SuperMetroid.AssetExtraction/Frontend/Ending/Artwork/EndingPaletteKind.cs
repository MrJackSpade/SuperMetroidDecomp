namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>What one of the ending's colour resources is.</summary>
internal enum EndingPaletteKind
{
    /// <summary>A whole screen's colours.</summary>
    WholeScreen,

    /// <summary>One run of colours, loaded over a part of a screen's.</summary>
    Run,

    /// <summary>The logo's crossfade: each step's BG and OBJ colour pair, read backwards from each step's pointer.</summary>
    LogoCrossfade,
}
