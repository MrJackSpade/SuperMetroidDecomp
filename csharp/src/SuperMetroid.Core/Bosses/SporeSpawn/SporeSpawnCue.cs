namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>One cue of Spore Spawn's body animations, with the operands it carries.</summary>
internal abstract record SporeSpawnCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private SporeSpawnCue()
    {
    }

    /// <summary>Loads a death-sequence fade step.</summary>
    /// <param name="DeathPaletteStep">The death-sequence fade step a death-palette cue loads, zero through six.</param>
    internal sealed record LoadDeathPalette(int DeathPaletteStep) : SporeSpawnCue;

    /// <summary>Loads a death-sequence fade step as the fade target.</summary>
    /// <param name="DeathPaletteStep">The death-sequence fade step a death-palette cue loads, zero through six.</param>
    internal sealed record LoadDeathTargetPalette(int DeathPaletteStep) : SporeSpawnCue;

    /// <summary>Requests a sound.</summary>
    /// <param name="Sound">The sound a <see cref="QueueSound"/> cue queues.</param>
    internal sealed record QueueSound(SoundEffectId? Sound) : SporeSpawnCue;

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
