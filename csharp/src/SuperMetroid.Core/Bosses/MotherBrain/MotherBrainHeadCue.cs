namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A gameplay cue of Mother Brain's brain animations.</summary>
internal abstract record MotherBrainHeadCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private MotherBrainHeadCue()
    {
    }

    /// <summary>What <paramref name="cases"/> makes of the cue, through the case it is.</summary>
    /// <typeparam name="TResult">What the cue is made into.</typeparam>
    /// <param name="cases">What each case of the cue is made into.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract TResult Match<TContext, TResult>(MotherBrainHeadCueCases<TContext, TResult> cases, TContext context);

    /// <summary>Requests a sound: a roar, an attack cry or the onion rings.</summary>
    /// <param name="Effect">The sound requested.</param>
    internal sealed record PlaySound(SoundEffectId Effect) : MotherBrainHeadCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(MotherBrainHeadCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).PlaySound(this, context);
    }

    /// <summary>Lobs a bomb with its afterburn count.</summary>
    /// <param name="Afterburns">The afterburn count the bomb lobs with.</param>
    internal sealed record SpawnBomb(int Afterburns) : MotherBrainHeadCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(MotherBrainHeadCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).SpawnBomb(this, context);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(MotherBrainHeadCueKind Kind) : MotherBrainHeadCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(MotherBrainHeadCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Signal(this, context);
    }

    /// <summary>A cue that carries no values.</summary>
    internal static MotherBrainHeadCue Of(MotherBrainHeadCueKind kind) => new Signal(kind);

    /// <summary>Requests <paramref name="sound"/>.</summary>
    internal static MotherBrainHeadCue Sound(SoundEffectId sound) => new PlaySound(sound);

    /// <summary>Lobs a bomb with <paramref name="afterburns"/> afterburns.</summary>
    internal static MotherBrainHeadCue Bomb(int afterburns) => new SpawnBomb(afterburns);
}

/// <summary>What each case of a <see cref="MotherBrainHeadCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the cue is made into.</typeparam>
/// <param name="PlaySound">Makes a <see cref="MotherBrainHeadCue.PlaySound"/> into the result.</param>
/// <param name="SpawnBomb">Makes a <see cref="MotherBrainHeadCue.SpawnBomb"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="MotherBrainHeadCue.Signal"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record MotherBrainHeadCueCases<TContext, TResult>(Func<MotherBrainHeadCue.PlaySound, TContext, TResult> PlaySound, Func<MotherBrainHeadCue.SpawnBomb, TContext, TResult> SpawnBomb, Func<MotherBrainHeadCue.Signal, TContext, TResult> Signal);
