namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>What a cue of the Shaktool's programs does: one per bank-$AA instruction its lists use.</summary>
internal enum ShaktoolCueKind
{
    /// <summary>Lowers the segment one pixel away from its target angle (<c>Instruction_Shaktool_Lower1Pixel</c>, $AA:D94A).</summary>
    Lower,
    /// <summary>Lowers the segment one pixel away from the head's facing (<c>UNUSED_Instruction_Shaktool_Lower1PixelAwayFromProj_AAD931</c>, $AA:D931).</summary>
    LowerAwayFromCenter,
    /// <summary>Does nothing: the native RTL stub between the two long attack pauses (<c>RTL_AAD99F</c>, $AA:D99F).</summary>
    Nothing,
    /// <summary>Raises the segment one pixel toward its target angle (<c>Instruction_Shaktool_Raise1Pixel</c>, $AA:D953).</summary>
    Raise,
    /// <summary>Raises the segment one pixel toward the head's facing (<c>UNUSED_Instruction_Shaktool_Raise1PixelTowardsProj_AAD93F</c>, $AA:D93F).</summary>
    RaiseTowardCenter,
    /// <summary>Restores every segment's authored pre-instruction (<c>Instruction_Shaktool_ResetShaktoolFunctions</c>, $AA:D9BA).</summary>
    ResetFunctions,
}
