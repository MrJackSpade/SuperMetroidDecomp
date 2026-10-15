namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>One cue of the Sidehoppers' and Dessgeegas' programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Value">The cue's word operand.</param>
internal readonly record struct HopperCue(HopperCueKind Kind, ushort Value = 0);
