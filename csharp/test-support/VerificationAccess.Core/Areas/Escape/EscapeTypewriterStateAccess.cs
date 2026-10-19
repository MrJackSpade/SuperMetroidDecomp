using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Areas.Escape;

/// <summary>Steps escape typewriters into a test VRAM image, for tests that check glyphs at their program destinations.</summary>
internal static class EscapeTypewriterStateAccess
{
    /// <summary>Typewriter stepping that writes each typed glyph into test VRAM.</summary>
    extension(EscapeTypewriterState state)
    {
        /// <summary>Steps the typewriter and writes the glyph it types, if any, into <paramref name="vram"/> at its program destination.</summary>
        [PpuLayoutBoundary("test fixture: writes typed glyphs at their programs' native VRAM destinations")]
        internal bool StepInto(SnesVram vram)
        {
            bool completed = state.Step(out EscapeTypewriterGlyph? glyph);
            if (glyph is { } typed)
                vram.ExecuteWordTransfer([typed.Tile.Raw], checked((ushort)typed.Destination), 1);
            return completed;
        }
    }
}
