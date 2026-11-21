namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>One cue of the Alcoon's animations: what it does, with the values it acts on.</summary>
internal abstract partial record AlcoonCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private AlcoonCue()
    {
    }

    /// <summary>What <paramref name="cases"/> makes of the cue, through the case it is.</summary>
    /// <typeparam name="TResult">What the cue is made into.</typeparam>
    /// <param name="cases">What each case of the cue is made into.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract TResult Match<TContext, TResult>(AlcoonCueCases<TContext, TResult> cases, TContext context);

    /// <summary>Spits a fireball by the cue's launch and plays the fire sound (#2151).</summary>
    /// <param name="Launch">How the fireball leaves: down, straight ahead or up.</param>
    internal sealed partial record Fire(AlcoonFireballLaunch Launch) : AlcoonCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(AlcoonCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Fire(this, context);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(AlcoonCueKind Kind) : AlcoonCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(AlcoonCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Signal(this, context);
    }
}

/// <summary>What each case of a <see cref="AlcoonCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the cue is made into.</typeparam>
/// <param name="Fire">Makes a <see cref="AlcoonCue.Fire"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="AlcoonCue.Signal"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record AlcoonCueCases<TContext, TResult>(Func<AlcoonCue.Fire, TContext, TResult> Fire, Func<AlcoonCue.Signal, TContext, TResult> Signal);
