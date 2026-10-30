using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Areas.Escape;

/// <summary>Steps escape typewriters into a test word image, for tests that check glyphs at their overlay cells.</summary>
internal static class EscapeTypewriterStateAccess
{
    /// <summary>Typewriter stepping that writes each typed glyph into test VRAM.</summary>
    extension(EscapeTypewriterState state)
    {
        /// <summary>
        /// Steps the typewriter and writes the tile of the glyph it types, if any, into <paramref name="vram"/>'s word at its
        /// overlay cell: the tile the import pictures the glyph from, over its program's tile base.
        /// </summary>
        internal bool StepInto(SnesVram vram)
        {
            bool completed = state.Step(out EscapeTypewriterGlyph? glyph);
            if (glyph is { } typed)
                vram.ExecuteWordTransfer([typed.TileOver(TileBaseOf(state.ProgramId)).PackedWord], checked((ushort)typed.Cell), 1);
            return completed;
        }
    }

    /// <summary>The tile a typed glyph stands for.</summary>
    extension(EscapeTypewriterGlyph glyph)
    {
        /// <summary>
        /// The tile the import pictures this glyph from (#1466): its character, the last of its picture's name, over
        /// <paramref name="tileBase"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">The character types no glyph.</exception>
        internal BackgroundTile TileOver(BackgroundTile tileBase) =>
            EscapeTypewriterState.GlyphOf(glyph.Picture[^1], tileBase)
                ?? throw new InvalidOperationException($"Typed picture {glyph.Picture} names a character that types no glyph.");
    }

    /// <summary>The tile base the game types <paramref name="program"/> from.</summary>
    private static BackgroundTile TileBaseOf(EscapeTypewriterProgramId program) => program switch
    {
        EscapeTypewriterProgramId.Ceres => EscapeTypewriterDefinitions.CeresTileBase,
        EscapeTypewriterProgramId.Zebes => EscapeTypewriterDefinitions.ZebesTileBase,
        EscapeTypewriterProgramId.None => throw new ArgumentOutOfRangeException(nameof(program), program, "No escape typewriter types this program."),
        _ => throw new ArgumentOutOfRangeException(nameof(program), program, "No escape typewriter types this program."),
    };
}
