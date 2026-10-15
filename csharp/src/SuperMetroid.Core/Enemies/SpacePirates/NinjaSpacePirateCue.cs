namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One cue of the ninja Space Pirates' programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Value">The cue's word operand.</param>
/// <param name="Offset">The cue's signed pixel offset.</param>
/// <param name="OffsetY">The cue's second, vertical signed pixel offset.</param>
/// <param name="Function">The cue's Function operand.</param>
internal readonly record struct NinjaSpacePirateCue(NinjaSpacePirateCueKind Kind, ushort Value = 0, short Offset = 0, short OffsetY = 0, NinjaSpacePirateFunction Function = default);
