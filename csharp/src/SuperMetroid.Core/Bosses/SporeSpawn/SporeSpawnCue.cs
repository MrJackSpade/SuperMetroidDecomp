namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>One cue of Spore Spawn's body animations: what it does, with the values it acts on.</summary>
internal abstract record SporeSpawnCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private SporeSpawnCue()
    {
    }

    /// <summary>
    /// Shows a step of his death fade over his body and his room: each step of his hardening, and the last as a defeated
    /// room loads him, which the door's fade then brings in.
    /// </summary>
    /// <param name="DeathShadeStep">The step of his death fade the cue shows.</param>
    internal sealed record ShowDeathShade(SporeSpawnDeathFadeStep DeathShadeStep) : SporeSpawnCue;

    /// <summary>Requests a sound.</summary>
    /// <param name="Sound">The sound a <see cref="QueueSound"/> cue queues.</param>
    internal sealed record QueueSound(SoundEffectId Sound) : SporeSpawnCue;

    /// <summary>Selects the main-AI movement.</summary>
    /// <param name="Movement">The main-AI movement a <see cref="SetMovement"/> cue selects.</param>
    internal sealed record SetMovement(SporeSpawnMovement Movement) : SporeSpawnCue;

    /// <summary>Sets the sway's maximum X radius and angle delta.</summary>
    /// <param name="MaximumXRadius">The sway's horizontal amplitude in room pixels a <see cref="SetMaxXRadiusAndAngleDelta"/> cue sets.</param>
    /// <param name="AngleDelta">The signed sway step in 256ths of a turn a <see cref="SetMaxXRadiusAndAngleDelta"/> cue sets.</param>
    internal sealed record SetMaxXRadiusAndAngleDelta(int MaximumXRadius, int AngleDelta) : SporeSpawnCue;

    /// <summary>Starts or stops spore generation.</summary>
    /// <param name="SporeGenerationSuppressed">Whether a <see cref="SetSporeGeneration"/> cue stops the ceiling spore emitters.</param>
    [IndependentFlag("whether an authored cue suppresses spore generation, a fact of the cue data")]
    internal sealed record SetSporeGeneration(bool SporeGenerationSuppressed) : SporeSpawnCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(SporeSpawnCueKind Kind) : SporeSpawnCue;
}
