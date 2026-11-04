namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// What Kraid's room backdrop shows (#1512): colours it has settled on, or a fade's colours, which are the colours the fade
/// started from after its steps toward its goal. A fade started during another starts from that fade's colours, so the
/// colours a fade shows are the chain of fades that led to them.
/// </summary>
internal abstract record KraidBackdropShown
{
    /// <summary>Closes the hierarchy to the kinds declared here.</summary>
    private KraidBackdropShown()
    {
    }

    /// <summary>
    /// What a fade toward <paramref name="goal"/> shows after its <paramref name="steps"/>, started from
    /// <paramref name="from"/>: the colours it started from while its steps leave them unchanged (a fade toward the colours
    /// it starts from, or only the step at counter zero), its goal once the step at the last moving value has landed on it,
    /// and the fade's colours otherwise, which a fade that completes without that step keeps.
    /// </summary>
    /// <param name="from">What the backdrop showed when the fade started.</param>
    /// <param name="goal">The colours the fade ends on.</param>
    /// <param name="steps">The counter's values at the fade's steps.</param>
    internal static KraidBackdropShown Fade(KraidBackdropShown from, KraidBackdropSettledColours goal, KraidBackdropFadeSteps steps)
    {
        Ensure.NotNull(from);
        if (!Enum.IsDefined(goal))
            throw new ArgumentOutOfRangeException(nameof(goal), goal, "Undefined Kraid backdrop colours.");
        if (from == new Settled(goal) || steps.Took(KraidBackgroundDefinitions.BackdropFadeLastStep))
            return new Settled(goal);
        if (steps.OnlyAtZero)
            return from;
        return new Fading(from, goal, steps);
    }

    /// <summary>Colours the backdrop has settled on.</summary>
    /// <param name="Colours">The colours shown.</param>
    internal sealed record Settled(KraidBackdropSettledColours Colours) : KraidBackdropShown;

    /// <summary>
    /// A fade's colours: <paramref name="From"/>'s after the shared gradual colour change's steps toward
    /// <paramref name="Goal"/> at the counter's values <paramref name="Steps"/>. Made only by <see cref="Fade"/>, so it never
    /// shows colours another kind names.
    /// </summary>
    /// <param name="From">What the backdrop showed when the fade started.</param>
    /// <param name="Goal">The colours the fade ends on.</param>
    /// <param name="Steps">The counter's values at the fade's steps, none at the last moving value.</param>
    internal sealed record Fading(KraidBackdropShown From, KraidBackdropSettledColours Goal, KraidBackdropFadeSteps Steps) : KraidBackdropShown;
}
