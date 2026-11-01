using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>The extents of the installed opening artwork the import composes the opening's pictures from.</summary>
internal static class IntroArtworkExtentDefinitions
{
    /// <summary>The scenes' 1024 four-bit background characters, in bytes.</summary>
    internal const int BackgroundCharacterBytes = 1024 * 32;

    /// <summary>The cells of one 32-by-32 narration tilemap.</summary>
    internal const int NarrationTilemapCells = 32 * 32;

    /// <summary>The 96 two-bit characters the Japanese narration blanks with its blank glyph, in bytes.</summary>
    internal const int JapaneseBlankCharactersByteCount = 96 * 16;

    /// <summary>Where the Japanese blank glyph starts in the font: its 41st character, in bytes.</summary>
    internal const int JapaneseBlankSourceOffset = 41 * 16;

    /// <summary>Cells copied for the final text line.</summary>
    internal const int FinalLineWordCount = 128;

    /// <summary>Portrait-eye rectangle size in tiles: three columns by two rows.</summary>
    internal static Extent16<TilemapCells> EyeFrameSize => new(3, 2);
}
