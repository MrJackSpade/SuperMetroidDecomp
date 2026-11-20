namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>One cue of the Alcoon's animations: what it does, with the values it acts on.</summary>
internal abstract record AlcoonCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private AlcoonCue()
    {
    }

    /// <summary>What the function for this case makes of the cue, whichever case it is.</summary>
    /// <typeparam name="TResult">What the cue is made into.</typeparam>
    /// <param name="fire">Makes a <see cref="Fire"/> into the result.</param>
    /// <param name="signal">Makes a <see cref="Signal"/> into the result.</param>
    internal abstract TResult Match<TResult>(Func<Fire, TResult> fire, Func<Signal, TResult> signal);

    /// <summary>Spits a fireball by the cue's launch and plays the fire sound (#2151).</summary>
    /// <param name="Launch">How the fireball leaves: down, straight ahead or up.</param>
    internal sealed record Fire(AlcoonFireballLaunch Launch) : AlcoonCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<Fire, TResult> fire, Func<Signal, TResult> signal) => Ensure.NotNull(fire)(this);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(AlcoonCueKind Kind) : AlcoonCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<Fire, TResult> fire, Func<Signal, TResult> signal) => Ensure.NotNull(signal)(this);
    }
}
