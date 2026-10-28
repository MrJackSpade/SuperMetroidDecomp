namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Character origins for the packed subtitle-arrow, baby, and caret drawings.</summary>
internal static class IntroScientistCellDefinitions
{
    /// <summary>Tile$1A8, first subtitle-arrow half in8C:8CCF.</summary>
    internal const int InitialArrow = 0x1a8;
    /// <summary>Tile$19F, top of the remaining two arrow drawings packed vertically.</summary>
    internal const int ArrowColumn = 0x19f;
    /// <summary>Tile$E9, start of three-wide top strips packed two per atlas row.</summary>
    internal const int DeliveredTop = 0xe9;
    /// <summary>Tile$E0, start of three adjacent three-by-two lower baby patches.</summary>
    internal const int DeliveredLower = 0xe0;
    /// <summary>Tile$199, first of three adjacent sixteen-pixel examined baby drawings.</summary>
    internal const int ExaminedStart = 0x199;
    /// <summary>Tile$FC, blinking typewriter block selected by8C:8D68.</summary>
    internal const int Caret = 0xfc;
}
