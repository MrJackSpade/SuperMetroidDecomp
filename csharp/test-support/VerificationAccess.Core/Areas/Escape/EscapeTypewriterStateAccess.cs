using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Assets;

using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Areas.Escape;

/// <summary>Steps escape typewriters into a test word image, for tests that check glyphs at the tilemap words the original game typed them into.</summary>
internal static class EscapeTypewriterStateAccess
{
    /// <summary>Typewriter stepping that writes each typed glyph into test VRAM.</summary>
    extension(EscapeTypewriterState state)
    {
        /// <summary>
        /// Steps the typewriter and writes the tile of the glyph it types, if any, into <paramref name="vram"/>'s word of the two
        /// tilemap screens its corner lies in: the tile the import pictures the glyph from, over its program's tile base.
        /// </summary>
        internal bool StepInto(SnesVram vram)
        {
            bool completed = state.Step(out EscapeTypewriterGlyph? glyph);
            if (glyph is { } typed)
                vram.ExecuteWordTransfer([EscapeTypewriterGlyphTilesConstants.Of(state.MessageId, typed).PackedWord], checked((ushort)SuperMetroid.Core.Rooms.Backgrounds.TwoScreenTilemapCells.IndexOf(typed.Corner)), 1);
            return completed;
        }
    }

}
