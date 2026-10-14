namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One cue of the walking Space Pirates' programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Value">The cue's word operand.</param>
/// <param name="Function">The cue's Function operand.</param>
internal readonly record struct WalkingSpacePirateCue(WalkingSpacePirateCueKind Kind, ushort Value = 0, WalkingSpacePirateFunction Function = default);
