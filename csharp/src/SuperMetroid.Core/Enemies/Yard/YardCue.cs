namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>One cue of the Yard's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Value">The cue's word operand.</param>
/// <param name="Offset">The cue's signed pixel offset.</param>
/// <param name="OffsetY">The cue's second, vertical signed pixel offset.</param>
/// <param name="OptionalTarget">The program the cue names, or none where the native operand is the $CF5F sentinel.</param>
/// <param name="MovementFunction">The cue's MovementFunction operand.</param>
internal readonly record struct YardCue(YardCueKind Kind, ushort Value = 0, short Offset = 0, short OffsetY = 0, YardProgram? OptionalTarget = null, YardMovementFunction MovementFunction = default);
