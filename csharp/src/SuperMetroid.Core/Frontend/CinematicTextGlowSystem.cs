using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Frontend;

/// <summary>Ports $8B:97F7/$9828/$9849: eight independently aging glyph rectangles.</summary>
internal sealed class CinematicTextGlowSystem
{
    /// <summary>Eight native glow records age independently and are allocated from the highest free slot.</summary>
    private readonly GlowSlot[] slots = new GlowSlot[CinematicTextGlowRomData.SlotCount];

    /// <summary>Starts a glow over the text-tilemap rectangle at <paramref name="cell"/>; an occupied eight-slot pool silently drops the request.</summary>
    public void Spawn(Position16<TilemapCells> cell, Extent16<TilemapCells> size)
    {
        for (int i = slots.Length - 1; i >= 0; i--)
        {
            if (slots[i].Active) continue;
            slots[i] = new GlowSlot { Active = true, Cell = cell, Size = size, Timer = 1 };
            return;
        }
    }

    /// <summary>Advances active glyph rectangles when their WRAM timers expire, rewriting palette bits while preserving tile identity and ending after the final palette.</summary>
    public void Step(Span<ushort> tilemap)
    {
        for (int i = slots.Length - 1; i >= 0; i--)
        {
            ref GlowSlot slot = ref slots[i];
            if (!slot.Active || slot.Timer-- != 1) continue;
            for (int row = 0; row < slot.Size.Y; row++)
            for (int column = 0; column < slot.Size.X; column++)
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

    /// <summary>One independently timed tilemap rectangle in the cinematic glow pool.</summary>
    private struct GlowSlot
    {
        /// <summary>Whether this native slot currently participates in text glow updates.</summary>
        public bool Active;
        /// <summary>Upper-left text tilemap cell of the glow rectangle.</summary>
        public Position16<TilemapCells> Cell;
        /// <summary>Number of text cells recolored along each axis.</summary>
        public Extent16<TilemapCells> Size;
        /// <summary>Countdown and current palette phase for this rectangle.</summary>
        public ushort Timer, Palette;
    }
}
