using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Areas.Escape;

/// <summary>Steps escape typewriters into a test word image, for tests that check glyphs at their overlay cells.</summary>
internal static class EscapeTypewriterStateAccess
{
    /// <summary>Typewriter stepping that writes each typed glyph into test VRAM.</summary>
    extension(EscapeTypewriterState state)
    {
        /// <summary>Steps the typewriter and writes the glyph it types, if any, into <paramref name="vram"/>'s word at its overlay cell.</summary>
        internal bool StepInto(SnesVram vram)
        {
            bool completed = state.Step(out EscapeTypewriterGlyph? glyph);
            if (glyph is { } typed)
                vram.ExecuteWordTransfer([typed.Tile.PackedWord], checked((ushort)typed.Cell), 1);
            return completed;
        }
    }
}
