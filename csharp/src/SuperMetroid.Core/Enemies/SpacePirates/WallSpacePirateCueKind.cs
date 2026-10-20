namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>What a cue of the wall Space Pirates' programs does: one per bank-$B2 instruction its lists use.</summary>
internal enum WallSpacePirateCueKind
{
    /// <summary>Picks a random climb direction and continues at that left-wall climb (<c>Instruction_PirateWall_RandomlyChooseADirection_LeftWall</c>, $B2:EEA4).</summary>
    ChooseLeftWallDirection,
    /// <summary>Picks a random climb direction and continues at that right-wall climb (<c>Instruction_PirateWall_RandomlyChooseADirection_RightWall</c>, $B2:EEBC).</summary>
    ChooseRightWallDirection,
    /// <summary>Fires a laser left (<c>Instruction_PirateWall_FireLaserLeft</c>, $B2:EF2A).</summary>
    FireLaserLeft,
    /// <summary>Fires a laser right (<c>Instruction_PirateWall_FireLaserRight</c>, $B2:EF5D).</summary>
    FireLaserRight,
    /// <summary>Plays the attack sound (<c>Instruction_PirateWall_QueueSpacePirateAttackSFX</c>, $B2:EF93).</summary>
    PlayAttackSound,
    /// <summary>Prepares the jump to the left wall (<c>Instruction_PirateWall_PrepareWallJumpToLeft</c>, $B2:EEFD).</summary>
    PrepareJumpLeft,
    /// <summary>Prepares the jump to the right wall (<c>Instruction_PirateWall_PrepareWallJumpToRight</c>, $B2:EED4).</summary>
    PrepareJumpRight,
}
