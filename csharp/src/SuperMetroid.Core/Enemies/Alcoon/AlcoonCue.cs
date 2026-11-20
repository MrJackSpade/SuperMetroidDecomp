namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>One cue of the Alcoon's animations: what it does, with the values it acts on.</summary>
internal abstract record AlcoonCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private AlcoonCue()
    {
    }

    /// <summary>What <paramref name="cases"/> makes of the cue, through the case it is.</summary>
    /// <typeparam name="TResult">What the cue is made into.</typeparam>
    /// <param name="cases">What each case of the cue is made into.</param>
    internal abstract TResult Match<TResult>(AlcoonCueCases<TResult> cases);

    /// <summary>Spits a fireball by the cue's launch and plays the fire sound (#2151).</summary>
    /// <param name="Launch">How the fireball leaves: down, straight ahead or up.</param>
    internal sealed record Fire(AlcoonFireballLaunch Launch) : AlcoonCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(AlcoonCueCases<TResult> cases) => Ensure.NotNull(cases).Fire(this);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(AlcoonCueKind Kind) : AlcoonCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(AlcoonCueCases<TResult> cases) => Ensure.NotNull(cases).Signal(this);
    }
}

/// <summary>What each case of a <see cref="AlcoonCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the cue is made into.</typeparam>
/// <param name="Fire">Makes a <see cref="AlcoonCue.Fire"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="AlcoonCue.Signal"/> into the result.</param>
internal sealed record AlcoonCueCases<TResult>(Func<AlcoonCue.Fire, TResult> Fire, Func<AlcoonCue.Signal, TResult> Signal);
