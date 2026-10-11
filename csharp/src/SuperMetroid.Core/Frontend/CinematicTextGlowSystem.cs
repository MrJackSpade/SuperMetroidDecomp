using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Frontend;

/// <summary>Ports $8B:97F7/$9828/$9849: eight independently aging glyph rectangles.</summary>
internal sealed class CinematicTextGlowSystem
{
    private readonly GlowSlot[] slots = new GlowSlot[CinematicTextGlowRomData.SlotCount];

    /// <summary>Starts a glow over the text-tilemap rectangle at <paramref name="cell"/>.</summary>
    public void Spawn(Point32<TilemapCells> cell, Size32<TilemapCells> size)
    {
        for (int i = slots.Length - 1; i >= 0; i--)
        {
            if (slots[i].Active) continue;
            slots[i] = new GlowSlot { Active = true, Cell = cell, Size = size, Timer = 1 };
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
            for (int row = 0; row < slot.Size.Height; row++)
            for (int column = 0; column < slot.Size.Width; column++)
            {
                int index = (slot.Cell.Y + row) * CinematicTextGlowRomData.RowWidth + slot.Cell.X + column;
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
        public Point32<TilemapCells> Cell;
        public Size32<TilemapCells> Size;
        public ushort Timer, Palette;
    }
}
