namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>One cue of Cacatac's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Value">The cue's word operand.</param>
internal readonly record struct CacatacCue(CacatacCueKind Kind, ushort Value = 0);
