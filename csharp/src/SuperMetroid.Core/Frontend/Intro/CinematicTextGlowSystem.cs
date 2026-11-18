using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The glows of typed narration letters, each aging on its own (#1625): every typed letter starts one, and none is
/// dropped or displaces another. Each recolours only its own letter, so the order they step in is not observable. Each
/// glow plays <see cref="IntroTextGlowAnimations.Glowing"/> on the shared animation player (#2488).
/// </summary>
internal sealed class CinematicTextGlowSystem
{
    /// <summary>The glows still running, oldest first.</summary>
    private readonly List<Glow> glows = [];

    /// <summary>Starts a glow over the letter at <paramref name="column"/> of row <paramref name="row"/>, which shows its first step on its first update.</summary>
    public void Spawn(int column, int row) => glows.Add(new Glow(column, row,
        AnimationPlayer<IntroTextGlowSegment, IntroTextGlowStep, NoAnimationCue>.Starting(IntroTextGlowAnimations.Segments, IntroTextGlowAnimations.Glowing)));

    /// <summary>
    /// Runs each glow one update, giving the letter it covers each step the glow shows anew while keeping the letter, and
    /// ends each glow once it rests on its last step.
    /// </summary>
    public void Step(IntroNarrationText text)
    {
        Ensure.NotNull(text);
        foreach (Glow glow in glows)
        {
            if (glow.Steps.StepToNewFrame() is { } step)
                text.Recolour(glow.Column, glow.Row, step);
        }
        _ = glows.RemoveAll(glow => glow.Steps.IsResting);
    }

    /// <summary>One independently timed letter glow.</summary>
    /// <param name="Column">The glowing letter's column of the narration's text rows.</param>
    /// <param name="Row">The glowing letter's row, counted from the narration's top.</param>
    /// <param name="Steps">The glow's steps as they play.</param>
    private sealed record Glow(int Column, int Row, AnimationPlayer<IntroTextGlowSegment, IntroTextGlowStep, NoAnimationCue> Steps);
}
