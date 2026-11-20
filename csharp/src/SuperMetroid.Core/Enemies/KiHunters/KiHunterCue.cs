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
    internal abstract TResult Match<TResult>(KiHunterCueCases<TResult> cases);

    /// <summary>Spits acid the cue's way and starts the spit wait (#2151).</summary>
    /// <param name="Direction">The way the acid flies.</param>
    internal sealed record Spit(HorizontalDirection Direction) : KiHunterCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(KiHunterCueCases<TResult> cases) => Ensure.NotNull(cases).Spit(this);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(KiHunterCueKind Kind) : KiHunterCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(KiHunterCueCases<TResult> cases) => Ensure.NotNull(cases).Signal(this);
    }
}

/// <summary>What each case of a <see cref="KiHunterCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the cue is made into.</typeparam>
/// <param name="Spit">Makes a <see cref="KiHunterCue.Spit"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="KiHunterCue.Signal"/> into the result.</param>
internal sealed record KiHunterCueCases<TResult>(Func<KiHunterCue.Spit, TResult> Spit, Func<KiHunterCue.Signal, TResult> Signal);
