using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;

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

    /// <summary>The tile <paramref name="character"/> is pictured from as <paramref name="program"/>'s typewriter types it.</summary>
    /// <exception cref="InvalidOperationException">The character types no glyph.</exception>
    internal static BackgroundTile Of(EscapeTypewriterProgramId program, char character) =>
        GlyphOf(character, TileBaseOf(program))
            ?? throw new InvalidOperationException($"The {program} typewriter types no glyph for '{character}'.");

    /// <summary>The tile a typed <paramref name="glyph"/> is pictured from: its character, the last of its picture's name.</summary>
    /// <exception cref="InvalidOperationException">The picture names a character that types no glyph.</exception>
    internal static BackgroundTile Of(EscapeTypewriterProgramId program, EscapeTypewriterGlyph glyph) =>
        Of(program, Ensure.NotNull(glyph.Picture)[^1]);
}
