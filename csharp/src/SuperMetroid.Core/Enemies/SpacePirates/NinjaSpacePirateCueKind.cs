namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>What a cue of the ninja Space Pirates' programs does: one per bank-$B2 instruction its lists use.</summary>
internal enum NinjaSpacePirateCueKind
{
    /// <summary>Resets the speed (<c>Instruction_PirateNinja_ResetSpeed</c>, $B2:F5D6).</summary>
    ResetSpeed,
    /// <summary>Continues at the active stance facing Samus (<c>Instruction_PirateNinja_SetFunction0FAC_Active</c>, $B2:F590).</summary>
    StartActive,
    /// <summary>Starts the leftward divekick jump (<c>Instruction_PirateNinja_SetLeftDivekickJumpInitialYSpeed</c>, $B2:F969).</summary>
    StartLeftDivekick,
    /// <summary>Starts the rightward divekick jump (<c>Instruction_PirateNinja_SetRightDivekickJumpInitialYSpeed</c>, $B2:FA3D).</summary>
    StartRightDivekick,
}
