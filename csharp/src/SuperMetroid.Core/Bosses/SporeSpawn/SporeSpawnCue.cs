namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>One cue of Spore Spawn's body programs, with the operands its native instruction carries.</summary>
internal abstract record SporeSpawnCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private SporeSpawnCue()
    {
    }

    /// <summary>Loads a death-sequence palette row (<c>Instruction_SporeSpawn_LoadDeathSequencePalette</c>, $A5:E8CA).</summary>
    /// <param name="DeathPaletteStep">The death-sequence palette row a death-palette cue loads, zero through six.</param>
    internal sealed record LoadDeathPalette(int DeathPaletteStep) : SporeSpawnCue;

    /// <summary>Loads a death-sequence palette row as the fade target (<c>Instruction_SporeSpawn_LoadDeathSequenceTargetPalette</c>, $A5:E91C).</summary>
    /// <param name="DeathPaletteStep">The death-sequence palette row a death-palette cue loads, zero through six.</param>
    internal sealed record LoadDeathTargetPalette(int DeathPaletteStep) : SporeSpawnCue;

    /// <summary>Queues a library-two sound (<c>Instruction_SporeSpawn_QueueSFXInY_Lib2_Max6</c>, $A5:E895).</summary>
    /// <param name="Sound">The sound a <see cref="QueueSound"/> cue queues.</param>
    internal sealed record QueueSound(SoundEffectId? Sound) : SporeSpawnCue;

    /// <summary>Selects the main-AI function (<c>Instruction_SporeSpawn_FunctionInY</c>, $A5:E8BA).</summary>
    /// <param name="Function">The main-AI function a <see cref="SetFunction"/> cue selects.</param>
    internal sealed record SetFunction(SporeSpawnFunction Function) : SporeSpawnCue;

    /// <summary>Sets the sway's maximum X radius and angle delta (<c>Instruction_SporeSpawn_SetMaxXRadiusAndAngleDelta</c>, $A5:E82D).</summary>
    /// <param name="MaximumXRadius">The sway's horizontal amplitude in room pixels a <see cref="SetMaxXRadiusAndAngleDelta"/> cue sets.</param>
    /// <param name="AngleDelta">The signed sway step in 256ths of a turn a <see cref="SetMaxXRadiusAndAngleDelta"/> cue sets.</param>
    internal sealed record SetMaxXRadiusAndAngleDelta(int MaximumXRadius, int AngleDelta) : SporeSpawnCue;

    /// <summary>Starts or stops spore generation (<c>Instruction_SporeSpawn_SporeGenerationFlagInY</c>, $A5:E872).</summary>
    /// <param name="SporeGenerationSuppressed">Whether a <see cref="SetSporeGeneration"/> cue stops the ceiling spore emitters.</param>
    [IndependentFlag("whether an authored cue suppresses spore generation, a fact of the cue data")]
    internal sealed record SetSporeGeneration(bool SporeGenerationSuppressed) : SporeSpawnCue;

    /// <summary>A cue that carries no operand.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Command(SporeSpawnCueKind Kind) : SporeSpawnCue;
}
