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
    /// What a fade toward <paramref name="goal"/> shows after its steps at the shared counter's values
    /// <paramref name="first"/> through <paramref name="reached"/>, started from <paramref name="from"/>: the colours it
    /// started from while its steps leave them unchanged (a fade toward the colours it starts from, or only the step at
    /// counter zero), its goal once the step at the last moving value has landed on it, and the fade's colours otherwise.
    /// </summary>
    /// <param name="from">What the backdrop showed when the fade started.</param>
    /// <param name="goal">The colours the fade ends on.</param>
    /// <param name="first">The counter's value at the fade's first step.</param>
    /// <param name="reached">The counter's value at the fade's latest step.</param>
    internal static KraidBackdropShown Fade(KraidBackdropShown from, KraidBackdropSettledColours goal, int first, int reached)
    {
        Ensure.NotNull(from);
        if (!Enum.IsDefined(goal))
            throw new ArgumentOutOfRangeException(nameof(goal), goal, "Undefined Kraid backdrop colours.");
        Ensure.BetweenInclusive(0, KraidBackgroundDefinitions.BackdropFadeLastStep, first);
        Ensure.BetweenInclusive(first, KraidBackgroundDefinitions.BackdropFadeLastStep, reached);
        if (from == new Settled(goal) || reached == KraidBackgroundDefinitions.BackdropFadeLastStep)
            return new Settled(goal);
        if (reached == 0)
            return from;
        return new Fading(from, goal, first, reached);
    }

    /// <summary>Colours the backdrop has settled on.</summary>
    /// <param name="Colours">The colours shown.</param>
    internal sealed record Settled(KraidBackdropSettledColours Colours) : KraidBackdropShown;

    /// <summary>
    /// A fade's colours: <paramref name="From"/>'s after the shared gradual colour change's steps toward
    /// <paramref name="Goal"/> at the counter's values <paramref name="First"/> through <paramref name="Reached"/>. Made only
    /// by <see cref="Fade"/>, so it never shows colours another kind names.
    /// </summary>
    /// <param name="From">What the backdrop showed when the fade started.</param>
    /// <param name="Goal">The colours the fade ends on.</param>
    /// <param name="First">The counter's value at the fade's first step.</param>
    /// <param name="Reached">The counter's value at the fade's latest step, below the last moving value.</param>
    internal sealed record Fading(KraidBackdropShown From, KraidBackdropSettledColours Goal, int First, int Reached) : KraidBackdropShown;
}
