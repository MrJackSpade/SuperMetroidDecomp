namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>What a cue of the walking Space Pirates' programs does: one per bank-$B2 instruction its lists use.</summary>
internal enum WalkingSpacePirateCueKind
{
    /// <summary>Continues at the walk or laser animation Samus's side and height select (<c>Instruction_PirateWalking_ChooseAMovement</c>, $B2:FCC8).</summary>
    ChooseMovement,
    /// <summary>Fires a laser left from the operand's height offset (<c>Instruction_PirateWalking_FireLaserLeftWithYOffsetInY</c>, $B2:FC68).</summary>
    FireLaserLeft,
    /// <summary>Fires a laser right from the operand's height offset (<c>Instruction_PirateWalking_FireLaserRightWithYOffsetInY</c>, $B2:FC90).</summary>
    FireLaserRight,
    /// <summary>Sets the function main AI runs next (<c>Instruction_PirateWalking_FunctionInY</c>, $B2:FCB8).</summary>
    SetFunction,
}
