using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Frontend;

/// <summary>Ports $8B:97F7/$9828/$9849: eight independently aging glyph rectangles.</summary>
internal sealed class CinematicTextGlowSystem
{
    /// <summary>Stores slots.</summary>
    private readonly GlowSlot[] slots = new GlowSlot[CinematicTextGlowRomData.SlotCount];

    /// <summary>Starts a glow over the text-tilemap rectangle at <paramref name="cell"/>.</summary>
    public void Spawn(Position16<TilemapCells> cell, Extent16<TilemapCells> size)
    {
        for (int i = slots.Length - 1; i >= 0; i--)
        {
            if (slots[i].Active) continue;
            slots[i] = new GlowSlot { Active = true, Cell = cell, Size = size, Timer = 1 };
            return;
        }
        // Native allocation simply returns when its fixed eight slots are occupied.
    }

    /// <summary>step.</summary>
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

    /// <summary>Represents glow slot.</summary>
    private struct GlowSlot
    {
        /// <summary>Stores active.</summary>
        public bool Active;
        /// <summary>Stores cell.</summary>
        public Position16<TilemapCells> Cell;
        /// <summary>Stores size.</summary>
        public Extent16<TilemapCells> Size;
        /// <summary>Stores timer.</summary>
        public ushort Timer, Palette;
    }
}
