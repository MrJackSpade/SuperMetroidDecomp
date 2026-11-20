namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A gameplay cue of Mother Brain's brain animations.</summary>
internal abstract record MotherBrainHeadCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private MotherBrainHeadCue()
    {
    }

    /// <summary>What the function for this case makes of the cue, whichever case it is.</summary>
    /// <typeparam name="TResult">What the cue is made into.</typeparam>
    /// <param name="playSound">Makes a <see cref="PlaySound"/> into the result.</param>
    /// <param name="spawnBomb">Makes a <see cref="SpawnBomb"/> into the result.</param>
    /// <param name="signal">Makes a <see cref="Signal"/> into the result.</param>
    internal abstract TResult Match<TResult>(Func<PlaySound, TResult> playSound, Func<SpawnBomb, TResult> spawnBomb, Func<Signal, TResult> signal);

    /// <summary>Requests a sound: a roar, an attack cry or the onion rings.</summary>
    /// <param name="Effect">The sound requested.</param>
    internal sealed record PlaySound(SoundEffectId Effect) : MotherBrainHeadCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<PlaySound, TResult> playSound, Func<SpawnBomb, TResult> spawnBomb, Func<Signal, TResult> signal) => Ensure.NotNull(playSound)(this);
    }

    /// <summary>Lobs a bomb with its afterburn count.</summary>
    /// <param name="Afterburns">The afterburn count the bomb lobs with.</param>
    internal sealed record SpawnBomb(int Afterburns) : MotherBrainHeadCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<PlaySound, TResult> playSound, Func<SpawnBomb, TResult> spawnBomb, Func<Signal, TResult> signal) => Ensure.NotNull(spawnBomb)(this);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(MotherBrainHeadCueKind Kind) : MotherBrainHeadCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<PlaySound, TResult> playSound, Func<SpawnBomb, TResult> spawnBomb, Func<Signal, TResult> signal) => Ensure.NotNull(signal)(this);
    }

    /// <summary>A cue that carries no values.</summary>
    internal static MotherBrainHeadCue Of(MotherBrainHeadCueKind kind) => new Signal(kind);

    /// <summary>Requests <paramref name="sound"/>.</summary>
    internal static MotherBrainHeadCue Sound(SoundEffectId sound) => new PlaySound(sound);

    /// <summary>Lobs a bomb with <paramref name="afterburns"/> afterburns.</summary>
    internal static MotherBrainHeadCue Bomb(int afterburns) => new SpawnBomb(afterburns);
}
