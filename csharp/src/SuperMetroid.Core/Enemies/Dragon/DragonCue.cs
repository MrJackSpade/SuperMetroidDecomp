namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>One cue of Dragon's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct DragonCue(DragonCueKind Kind);
