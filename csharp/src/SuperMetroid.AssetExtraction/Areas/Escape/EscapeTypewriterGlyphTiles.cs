using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>
/// The tile each escape typewriter glyph is pictured from (#1467): the import's knowledge of how the cartridge's
/// typewriters step a character's tile from their program's tile base. Gameplay names a typed glyph's picture; the import
/// and the checks that compare typed words read the tile here.
/// </summary>
internal static class EscapeTypewriterGlyphTiles
{
    /// <summary>The tile of the letter A <paramref name="program"/>'s typewriter types from.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No escape typewriter types <paramref name="program"/>.</exception>
    internal static BackgroundTile TileBaseOf(EscapeTypewriterProgramId program) => program switch
    {
        EscapeTypewriterProgramId.Ceres => EscapeTypewriterTileDefinitions.CeresTileBase,
        EscapeTypewriterProgramId.Zebes => EscapeTypewriterTileDefinitions.ZebesTileBase,
        EscapeTypewriterProgramId.None => throw new ArgumentOutOfRangeException(nameof(program), program, "No escape typewriter types this program."),
        _ => throw new ArgumentOutOfRangeException(nameof(program), program, "No escape typewriter types this program."),
    };

    /// <summary>The tile <paramref name="character"/> types from <paramref name="tileBase"/>, or null where it types no glyph.</summary>
    internal static BackgroundTile? GlyphOf(int character, BackgroundTile tileBase)
    {
        if (!EscapeTypewriterGlyph.Types((char)character))
            return null;
        if (character == (byte)'!') character = EscapeTypewriterTileDefinitions.ExclamationGlyph;
        return tileBase with { Tile = tileBase.Tile + character - EscapeTypewriterTileDefinitions.FirstLetter };
    }
}
