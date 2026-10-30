namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Ports $8B:97F7/$9828/$9849: eight independently aging glowing rectangles of narration text.</summary>
internal sealed class CinematicTextGlowSystem
{
    /// <summary>Eight native glow records age independently and are allocated from the highest free slot.</summary>
    private readonly GlowSlot[] slots = new GlowSlot[CinematicTextGlowDefinitions.SlotCount];

    /// <summary>Starts a glow over the text rectangle at <paramref name="cell"/>; an occupied eight-slot pool silently drops the request.</summary>
    public void Spawn(Position16<TilemapCells> cell, Extent16<TilemapCells> size)
    {
        for (int i = slots.Length - 1; i >= 0; i--)
        {
            if (slots[i].Active) continue;
            slots[i] = new GlowSlot { Active = true, Cell = cell, Size = size, Timer = WrappingCountdown.Start(1) };
            return;
        }
    }

    /// <summary>
    /// Advances active rectangles when their timers expire, giving each staged cell they cover the rectangle's current
    /// colour variant while keeping what it shows, and ending after the final variant.
    /// </summary>
    public void Step(IntroNarrationText text)
    {
        Ensure.NotNull(text);
        for (int i = slots.Length - 1; i >= 0; i--)
        {
            ref GlowSlot slot = ref slots[i];
            if (!slot.Active || !(slot.Timer = slot.Timer.Decrement().Remaining).IsZero) continue;
            for (int row = 0; row < slot.Size.Y; row++)
            for (int column = 0; column < slot.Size.X; column++)
                text.Recolour(new(checked((ushort)(slot.Cell.X + column)), checked((ushort)(slot.Cell.Y + row))), slot.Palette);
            if (slot.Palette == CinematicTextGlowDefinitions.FinalPalette)
                slot.Active = false;
            else
            {
                slot.Palette++;
                slot.Timer = WrappingCountdown.Start(CinematicTextGlowDefinitions.PaletteDuration);
            }
        }
    }

    /// <summary>One independently timed text rectangle in the cinematic glow pool.</summary>
    private struct GlowSlot
    {
        /// <summary>Whether this native slot currently participates in text glow updates.</summary>
        public bool Active;
        /// <summary>Upper-left text cell of the glow rectangle.</summary>
        public Position16<TilemapCells> Cell;
        /// <summary>Number of text cells recoloured along each axis.</summary>
        public Extent16<TilemapCells> Size;
        /// <summary>Frames until this rectangle's next colour phase.</summary>
        public WrappingCountdown Timer;
        /// <summary>
        /// Current colour variant given to the rectangle's cells: zero when the glow starts, raised by one each phase up to
        /// <see cref="CinematicTextGlowDefinitions.FinalPalette"/>, where the glow ends.
        /// </summary>
        public int Palette;
    }
}
