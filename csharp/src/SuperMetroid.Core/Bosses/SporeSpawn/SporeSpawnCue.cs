namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>One cue of Spore Spawn's body programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Function">The main-AI function a <see cref="SporeSpawnCueKind.SetFunction"/> cue selects.</param>
/// <param name="MaximumXRadius">The sway's horizontal amplitude in room pixels a <see cref="SporeSpawnCueKind.SetMaxXRadiusAndAngleDelta"/> cue sets.</param>
/// <param name="AngleDelta">The signed sway step in 256ths of a turn a <see cref="SporeSpawnCueKind.SetMaxXRadiusAndAngleDelta"/> cue sets.</param>
/// <param name="SporeGenerationSuppressed">Whether a <see cref="SporeSpawnCueKind.SetSporeGeneration"/> cue stops the ceiling spore emitters.</param>
/// <param name="Sound">The sound a <see cref="SporeSpawnCueKind.QueueSound"/> cue queues.</param>
/// <param name="DeathPaletteStep">The death-sequence palette row a death-palette cue loads, zero through six.</param>
internal readonly record struct SporeSpawnCue(
    SporeSpawnCueKind Kind,
    SporeSpawnFunction Function = default,
    int MaximumXRadius = 0,
    int AngleDelta = 0,
    bool SporeGenerationSuppressed = false,
    SoundEffectId? Sound = null,
    int DeathPaletteStep = 0);
