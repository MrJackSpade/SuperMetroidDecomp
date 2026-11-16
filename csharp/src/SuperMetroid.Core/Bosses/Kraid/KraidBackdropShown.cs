namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// What Kraid's room backdrop shows (#1512, #2215): colours it has settled on, or a fade partway from what it showed when
/// the fade started toward the colours the fade ends on. A fade started during another starts from that fade partway.
/// </summary>
internal abstract record KraidBackdropShown
{
    /// <summary>Closes the hierarchy to the kinds declared here.</summary>
    private KraidBackdropShown()
    {
    }

    /// <summary>
    /// What a fade toward <paramref name="goal"/> shows, started from <paramref name="from"/>, <paramref name="progress"/>
    /// of the way: what it started from before it has moved, its goal once it has come the whole way or when it fades
    /// toward the colours it started from, and the fade partway otherwise.
    /// </summary>
    /// <param name="from">What the backdrop showed when the fade started.</param>
    /// <param name="goal">The colours the fade ends on.</param>
    /// <param name="progress">How far the fade has come.</param>
    internal static KraidBackdropShown Fade(KraidBackdropShown from, KraidBackdropSettledColours goal, KraidBackdropFadeProgress progress)
    {
        Ensure.NotNull(from);
        if (from == new Settled(goal) || progress.IsComplete)
            return new Settled(goal);
        if (progress.Moved == 0)
            return from;
        return new Fading(from, goal, progress);
    }

    /// <summary>Colours the backdrop has settled on.</summary>
    /// <param name="Colours">The colours shown.</param>
    internal sealed record Settled(KraidBackdropSettledColours Colours) : KraidBackdropShown;

    /// <summary>
    /// A fade partway: from <paramref name="From"/> toward <paramref name="Goal"/>, <paramref name="Progress"/> of the way.
    /// Made only by <see cref="Fade"/>, so it never shows colours another kind names.
    /// </summary>
    /// <param name="From">What the backdrop showed when the fade started.</param>
    /// <param name="Goal">The colours the fade ends on.</param>
    /// <param name="Progress">How far the fade has come, neither none nor the whole way.</param>
    internal sealed record Fading(KraidBackdropShown From, KraidBackdropSettledColours Goal, KraidBackdropFadeProgress Progress) : KraidBackdropShown;
}
