namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>One cue of the Magdollite's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Value">The cue's word operand.</param>
internal readonly record struct MagdolliteCue(MagdolliteCueKind Kind, ushort Value = 0);
