namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>One cue of the Chozo statue's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Value">The cue's word operand.</param>
/// <param name="PreInstruction">The cue's PreInstruction operand.</param>
internal readonly record struct ChozoStatueCue(ChozoStatueCueKind Kind, ushort Value = 0, ChozoStatuePreInstruction PreInstruction = default);
