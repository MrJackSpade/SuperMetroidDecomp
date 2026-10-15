namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>What a cue of the ninja Space Pirates' programs does: one per bank-$B2 instruction its lists use.</summary>
internal enum NinjaSpacePirateCueKind
{
    /// <summary>Plays the operand's library-two sound (<c>Instruction_PirateNinja_QueueSoundInY_Lib2_Max6</c>, $B2:F546).</summary>
    PlaySound,
    /// <summary>Resets the speed (<c>Instruction_PirateNinja_ResetSpeed</c>, $B2:F5D6).</summary>
    ResetSpeed,
    /// <summary>Sets the function main AI runs next (<c>Instruction_PirateWall_FunctionInY</c>, $B2:EF83).</summary>
    SetFunction,
    /// <summary>Sets the palette index to the operand (<c>Instruction_PirateNinja_PaletteIndexInY</c>, $B2:F536).</summary>
    SetPalette,
    /// <summary>Continues at the active stance facing Samus (<c>Instruction_PirateNinja_SetFunction0FAC_Active</c>, $B2:F590).</summary>
    StartActive,
    /// <summary>Starts the leftward divekick jump (<c>Instruction_PirateNinja_SetLeftDivekickJumpInitialYSpeed</c>, $B2:F969).</summary>
    StartLeftDivekick,
    /// <summary>Starts the rightward divekick jump (<c>Instruction_PirateNinja_SetRightDivekickJumpInitialYSpeed</c>, $B2:FA3D).</summary>
    StartRightDivekick,
    /// <summary>Throws a claw in the operand's direction from the given offset (<c>Instruction_PirateNinja_SpawnClawProjWithThrowDirSpawnOffset</c>, $B2:F564).</summary>
    ThrowClaw,
}
