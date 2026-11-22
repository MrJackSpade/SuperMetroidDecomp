namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>The extents of the installed opening artwork the import composes the opening's pictures from.</summary>
internal static partial class IntroArtworkExtentDefinitions
{
    /// <summary>The scenes' 1024 four-bit background characters, in bytes.</summary>
    internal const int BackgroundCharacterBytes = 1024 * 32;

    /// <summary>
    /// The cells along each side of one of the opening's tilemap pages: its scene pages, its portrait and its narration
    /// pages, which the import pictures whole (#2204).
    /// </summary>
    internal const int PlaneCells = 32;

    /// <summary>The cells of one narration tilemap.</summary>
    internal const int NarrationTilemapCells = PlaneCells * PlaneCells;

    /// <summary>The 96 two-bit characters the Japanese narration blanks with its blank glyph, in bytes.</summary>
    internal const int JapaneseBlankCharactersByteCount = 96 * 16;

    /// <summary>Where the Japanese blank glyph starts in the font: its 41st character, in bytes.</summary>
    internal const int JapaneseBlankSourceOffset = 41 * 16;
}
