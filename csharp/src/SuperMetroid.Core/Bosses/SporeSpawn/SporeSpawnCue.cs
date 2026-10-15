namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>One cue of Spore Spawn's body programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Function">The main-AI function a <see cref="SporeSpawnCueKind.SetFunction"/> cue selects.</param>
/// <param name="Value">
/// The cue's word operand: the maximum X radius, spore-generation flag, sound or death-palette row step its kind
/// takes.
/// </param>
/// <param name="AngleDelta">The angle delta a <see cref="SporeSpawnCueKind.SetMaxXRadiusAndAngleDelta"/> cue sets.</param>
internal readonly record struct SporeSpawnCue(
    SporeSpawnCueKind Kind,
    SporeSpawnFunction Function = default,
    ushort Value = 0,
    ushort AngleDelta = 0);
