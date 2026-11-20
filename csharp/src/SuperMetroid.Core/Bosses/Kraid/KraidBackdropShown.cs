using SuperMetroid.Core.Fx.ColourEffects;

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

    /// <summary>What <paramref name="settled"/> or <paramref name="fading"/> makes of the shown colours, whichever they are.</summary>
    /// <typeparam name="TResult">What the colours are made into.</typeparam>
    /// <param name="settled">Makes settled colours into the result.</param>
    /// <param name="fading">Makes a fade into the result.</param>
    internal abstract TResult Match<TResult>(Func<Settled, TResult> settled, Func<Fading, TResult> fading);

    /// <summary>Carries out <paramref name="settled"/> or <paramref name="fading"/> on the shown colours, whichever they are (#2553).</summary>
    /// <typeparam name="TContext">What either reaction reads besides the colours.</typeparam>
    /// <param name="settled">Reacts to settled colours.</param>
    /// <param name="fading">Reacts to a fade.</param>
    /// <param name="context">What either reaction reads besides the colours, handed to it so neither is built per call.</param>
    internal abstract void React<TContext>(Action<Settled, TContext> settled, Action<Fading, TContext> fading, TContext context);

    /// <summary>
    /// What a fade toward <paramref name="goal"/> shows, started from <paramref name="from"/>, <paramref name="progress"/>
    /// of the way: what it started from before it has moved, its goal once it has come the whole way or when it fades
    /// toward the colours it started from, and the fade partway otherwise.
    /// </summary>
    /// <param name="from">What the backdrop showed when the fade started.</param>
    /// <param name="goal">The colours the fade ends on.</param>
    /// <param name="progress">How far the fade has come.</param>
    internal static KraidBackdropShown Fade(KraidBackdropShown from, KraidBackdropSettledColours goal, StepCountedFadeProgress progress)
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
    internal sealed record Settled(KraidBackdropSettledColours Colours) : KraidBackdropShown
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<Settled, TResult> settled, Func<Fading, TResult> fading) => Ensure.NotNull(settled)(this);

        /// <inheritdoc/>
        internal override void React<TContext>(Action<Settled, TContext> settled, Action<Fading, TContext> fading, TContext context) =>
            Ensure.NotNull(settled)(this, context);
    }

    /// <summary>
    /// A fade partway: from <paramref name="From"/> toward <paramref name="Goal"/>, <paramref name="Progress"/> of the way.
    /// Made only by <see cref="Fade"/>, so it never shows colours another kind names.
    /// </summary>
    /// <param name="From">What the backdrop showed when the fade started.</param>
    /// <param name="Goal">The colours the fade ends on.</param>
    /// <param name="Progress">How far the fade has come, neither none nor the whole way.</param>
    internal sealed record Fading(KraidBackdropShown From, KraidBackdropSettledColours Goal, StepCountedFadeProgress Progress) : KraidBackdropShown
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<Settled, TResult> settled, Func<Fading, TResult> fading) => Ensure.NotNull(fading)(this);

        /// <inheritdoc/>
        internal override void React<TContext>(Action<Settled, TContext> settled, Action<Fading, TContext> fading, TContext context) =>
            Ensure.NotNull(fading)(this, context);
    }
}
