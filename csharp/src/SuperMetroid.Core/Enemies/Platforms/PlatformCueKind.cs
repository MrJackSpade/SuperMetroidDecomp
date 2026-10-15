namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>What a cue of the Tripper's and Kamer's programs does: one per bank-$A3 instruction its lists use.</summary>
internal enum PlatformCueKind
{
    /// <summary>Makes main AI move the platform left (<c>Instruction_Tripper_Kamer2_SetMovingLeftXMovement</c>, $A3:9C6B, and its byte-identical duplicate at $A3:9C81).</summary>
    MoveLeft,
    /// <summary>Makes main AI move the platform right (<c>Instruction_Tripper_Kamer2_SetMovingRightXMovement</c>, $A3:9C76, and its byte-identical duplicate at $A3:9C8C).</summary>
    MoveRight,
}
