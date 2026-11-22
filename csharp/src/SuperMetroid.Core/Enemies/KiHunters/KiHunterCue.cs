namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>One cue of the Ki-Hunter's animations: what it does, with the values it acts on.</summary>
internal abstract record KiHunterCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private KiHunterCue()
    {
    }

    /// <summary>What <paramref name="cases"/> makes of the cue, through the case it is.</summary>
    /// <typeparam name="TResult">What the cue is made into.</typeparam>
    /// <param name="cases">What each case of the cue is made into.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract TResult Match<TContext, TResult>(KiHunterCueCases<TContext, TResult> cases, TContext context);

    /// <summary>Spits acid the cue's way and starts the spit wait (#2151).</summary>
    /// <param name="Direction">The way the acid flies.</param>
    internal sealed record Spit(HorizontalDirection Direction) : KiHunterCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(KiHunterCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Spit(this, context);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(KiHunterCueKind Kind) : KiHunterCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(KiHunterCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Signal(this, context);
    }
}
