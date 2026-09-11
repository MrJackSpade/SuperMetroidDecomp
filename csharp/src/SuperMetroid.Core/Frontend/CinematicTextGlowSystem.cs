namespace SuperMetroid.Core.Frontend;

/// <summary>Ports $8B:97F7/$9828/$9849: eight independently aging glyph rectangles.</summary>
internal sealed class CinematicTextGlowSystem
{
    private readonly GlowSlot[] slots = new GlowSlot[CinematicTextGlowRomData.SlotCount];

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

    private struct GlowSlot
    {
        public bool Active;
        public int X, Y, Width, Height;
        public ushort Timer, Palette;
    }
}
