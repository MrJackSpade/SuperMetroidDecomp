namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>One cue of Stoke's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Value">The cue's word operand.</param>
internal readonly record struct StokeCue(StokeCueKind Kind, ushort Value = 0);
