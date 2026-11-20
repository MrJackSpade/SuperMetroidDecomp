namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>One cue of the Ki-Hunter's animations: what it does, with the values it acts on.</summary>
internal abstract record KiHunterCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private KiHunterCue()
    {
    }

    /// <summary>What the function for this case makes of the cue, whichever case it is.</summary>
    /// <typeparam name="TResult">What the cue is made into.</typeparam>
    /// <param name="spit">Makes a <see cref="Spit"/> into the result.</param>
    /// <param name="signal">Makes a <see cref="Signal"/> into the result.</param>
    internal abstract TResult Match<TResult>(Func<Spit, TResult> spit, Func<Signal, TResult> signal);

    /// <summary>Spits acid the cue's way and starts the spit wait (#2151).</summary>
    /// <param name="Direction">The way the acid flies.</param>
    internal sealed record Spit(HorizontalDirection Direction) : KiHunterCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<Spit, TResult> spit, Func<Signal, TResult> signal) => Ensure.NotNull(spit)(this);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(KiHunterCueKind Kind) : KiHunterCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<Spit, TResult> spit, Func<Signal, TResult> signal) => Ensure.NotNull(signal)(this);
    }
}
