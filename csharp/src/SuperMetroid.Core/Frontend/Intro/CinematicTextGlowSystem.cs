namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Eight independently aging glows of typed narration letters.</summary>
internal sealed class CinematicTextGlowSystem
{
    /// <summary>Eight glow records age independently and are allocated from the highest free slot.</summary>
    private readonly GlowSlot[] slots = new GlowSlot[CinematicTextGlowDefinitions.SlotCount];

    /// <summary>
    /// Starts a glow over the letter at <paramref name="column"/> of row <paramref name="row"/>; an occupied eight-slot pool
    /// silently drops the request.
    /// </summary>
    public void Spawn(int column, int row)
    {
        for (int i = slots.Length - 1; i >= 0; i--)
        {
            if (slots[i].Active) continue;
            slots[i] = new GlowSlot { Active = true, Column = column, Row = row, Timer = 1 };
            return;
        }
    }

    /// <summary>
    /// Advances active glows when their timers expire, giving the letter each covers the glow's current colour variant
    /// while keeping the letter, and ending after the final variant.
    /// </summary>
    public void Step(IntroNarrationText text)
    {
        Ensure.NotNull(text);
        for (int i = slots.Length - 1; i >= 0; i--)
        {
            ref GlowSlot slot = ref slots[i];
            if (!slot.Active || --slot.Timer != 0) continue;
            text.Recolour(slot.Column, slot.Row, slot.ColourVariant);
            if (slot.ColourVariant == CinematicTextGlowDefinitions.FinalVariant)
                slot.Active = false;
            else
            {
                slot.ColourVariant++;
                slot.Timer = CinematicTextGlowDefinitions.VariantDuration;
            }
        }
    }

    /// <summary>One independently timed letter glow in the cinematic glow pool.</summary>
    private struct GlowSlot
    {
        /// <summary>Whether this slot currently participates in text glow updates.</summary>
        public bool Active;
        /// <summary>The glowing letter's column of the narration's text rows.</summary>
        public int Column;
        /// <summary>The glowing letter's row, counted from the narration's top.</summary>
        public int Row;
        /// <summary>Frames until this glow's next colour phase.</summary>
        public int Timer;
        /// <summary>
        /// Current colour variant given to the glow's letter: zero when the glow starts, raised by one each phase up to
        /// <see cref="CinematicTextGlowDefinitions.FinalVariant"/>, where the glow ends.
        /// </summary>
        public int ColourVariant;
    }
}
