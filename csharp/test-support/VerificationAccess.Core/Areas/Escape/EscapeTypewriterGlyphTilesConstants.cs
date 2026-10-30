using SuperMetroid.Core;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>Cartridge values and helpers of <see cref="EscapeTypewriterGlyphTiles"/> that only verification reads.</summary>
internal static class EscapeTypewriterGlyphTilesConstants
{
    /// <summary>The tile <paramref name="character"/> is pictured from as <paramref name="program"/>'s typewriter types it.</summary>
    /// <exception cref="InvalidOperationException">The character types no glyph.</exception>
    internal static BackgroundTile Of(EscapeTypewriterProgramId program, char character) =>
        EscapeTypewriterGlyphTiles.GlyphOf(character, EscapeTypewriterGlyphTiles.TileBaseOf(program))
            ?? throw new InvalidOperationException($"The {program} typewriter types no glyph for '{character}'.");
    /// <summary>The tile a typed <paramref name="glyph"/> is pictured from: its character, the last of its picture's name.</summary>
    /// <exception cref="InvalidOperationException">The picture names a character that types no glyph.</exception>
    internal static BackgroundTile Of(EscapeTypewriterProgramId program, EscapeTypewriterGlyph glyph) =>
        Of(program, Ensure.NotNull(glyph.Picture)[^1]);
}
