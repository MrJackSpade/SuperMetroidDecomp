namespace SuperMetroid.Core.Frontend;

/// <summary>Ports $8B:97F7/$9828/$9849: eight independently aging glyph rectangles.</summary>
internal sealed class CinematicTextGlowSystem
{
    /// <summary>Fixed-capacity slots for glyph rectangles currently progressing through the palette glow sequence.</summary>
    private readonly GlowSlot[] slots = new GlowSlot[CinematicTextGlowRomData.SlotCount];

    /// <summary>Adds a glyph rectangle to the last free slot, beginning at palette zero; a full slot pool ignores the request.</summary>
    /// <param name="x">Leftmost tilemap column covered by the rectangle.</param>
    /// <param name="y">Topmost tilemap row covered by the rectangle.</param>
    /// <param name="width">Number of tilemap columns to recolor on each palette step.</param>
    /// <param name="height">Number of tilemap rows to recolor on each palette step.</param>
    public void Spawn(int x, int y, int width, int height)
    {
        for (int i = slots.Length - 1; i >= 0; i--)
        {
            if (slots[i].Active) continue;
            slots[i] = new GlowSlot { Active = true, X = x, Y = y, Width = width, Height = height, Timer = 1 };
            return;
        }
        // Native allocation simply returns when its fixed eight slots are occupied.
    }

    /// <summary>Advances each active rectangle's timer and updates its covered tilemap words to the next glow palette.</summary>
    /// <param name="tilemap">Mutable row-major tilemap whose tile identity bits are preserved while palette bits change.</param>
    public void Step(Span<ushort> tilemap)
    {
        for (int i = slots.Length - 1; i >= 0; i--)
        {
            ref GlowSlot slot = ref slots[i];
            if (!slot.Active || slot.Timer-- != 1) continue;
            for (int row = 0; row < slot.Height; row++)
            for (int column = 0; column < slot.Width; column++)
            {
                int index = (slot.Y + row) * CinematicTextGlowRomData.RowWidth + slot.X + column;
                tilemap[index] = (ushort)((tilemap[index] & CinematicTextGlowRomData.PreserveTileBits) |
                    slot.Palette << CinematicTextGlowRomData.PaletteShift);
            }
            if (slot.Palette == CinematicTextGlowRomData.FinalPalette)
                slot.Active = false;
            else
            {
                slot.Palette++;
                slot.Timer = CinematicTextGlowRomData.PaletteDuration;
            }
        }
    }

    /// <summary>Stores the bounds and palette-cycle progress of one active glyph glow rectangle.</summary>
    private struct GlowSlot
    {
        /// <summary>Whether this slot currently owns a rectangle awaiting palette updates.</summary>
        public bool Active;

        /// <summary>Rectangle origin and extent in tilemap cells, in X, Y, width, and height order.</summary>
        public int X, Y, Width, Height;

        /// <summary>Ticks remaining until the next recolor and the palette index currently applied to the rectangle.</summary>
        public ushort Timer, Palette;
    }
}
