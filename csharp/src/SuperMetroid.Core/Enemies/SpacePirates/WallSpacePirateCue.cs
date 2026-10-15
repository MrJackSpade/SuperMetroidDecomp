namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One cue of the wall Space Pirates' programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Value">The cue's word operand.</param>
/// <param name="Function">The cue's Function operand.</param>
internal readonly record struct WallSpacePirateCue(WallSpacePirateCueKind Kind, ushort Value = 0, WallSpacePirateFunction Function = default);
