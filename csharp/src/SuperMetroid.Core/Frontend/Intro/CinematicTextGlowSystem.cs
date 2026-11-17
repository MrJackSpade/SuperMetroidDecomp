namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The glows of typed narration letters, each aging on its own (#1625): every typed letter starts one, and none is
/// dropped or displaces another. Each recolours only its own letter, so the order they step in is not observable.
/// </summary>
internal sealed class CinematicTextGlowSystem
{
    /// <summary>The glows still running, oldest first.</summary>
    private readonly List<Glow> glows = [];

    /// <summary>Starts a glow over the letter at <paramref name="column"/> of row <paramref name="row"/>.</summary>
    public void Spawn(int column, int row) => glows.Add(new Glow { Column = column, Row = row, Timer = 1 });

    /// <summary>
    /// Advances the glows whose timers expire, giving the letter each covers the glow's current step while
    /// keeping the letter, and ends each glow after its last step.
    /// </summary>
    public void Step(IntroNarrationText text)
    {
        Ensure.NotNull(text);
        foreach (Glow glow in glows)
        {
            if (--glow.Timer != 0) continue;
            text.Recolour(glow.Column, glow.Row, glow.Shown);
            if (glow.Shown.Following is { } following)
            {
                glow.Shown = following;
                glow.Timer = CinematicTextGlowDefinitions.VariantDuration;
            }
            else
                glow.Ended = true;
        }
        _ = glows.RemoveAll(glow => glow.Ended);
    }

    /// <summary>One independently timed letter glow.</summary>
    private sealed class Glow
    {
        /// <summary>The glowing letter's column of the narration's text rows.</summary>
        public int Column;
        /// <summary>The glowing letter's row, counted from the narration's top.</summary>
        public int Row;
        /// <summary>Frames until this glow's next colour phase.</summary>
        public int Timer;
        /// <summary>The glow step its letter is given next: the first when the glow starts, then each in turn up to the last, after which the glow ends.</summary>
        public IntroTextGlowStep Shown = IntroTextGlowStep.Typed;
        /// <summary>Whether the glow has shown its final variant, so the system drops it.</summary>
        public bool Ended;
    }
}
