namespace SuperMetroid.Core.Game;

/// <summary>Space Pirate private bank-$B2 instruction opcodes.</summary>
internal enum SpacePirateInstruction : ushort
{
    /// <summary><c>Instruction_PirateWall_FunctionInY</c> at $B2:EF83.</summary>
    PirateWall_FunctionInY = 0xef83,

    /// <summary><c>Instruction_PirateNinja_PaletteIndexInY</c> at $B2:F536.</summary>
    PirateNinja_PaletteIndexInY = 0xf536,

    /// <summary><c>Instruction_PirateNinja_QueueSoundInY_Lib2_Max6</c> at $B2:F546.</summary>
    PirateNinja_QueueSoundInY_Lib2_Max6 = 0xf546,

    /// <summary><c>Instruction_PirateNinja_SpawnClawProjWithThrowDirSpawnOffset</c> at $B2:F564.</summary>
    PirateNinja_SpawnClawProjWithThrowDirSpawnOffset = 0xf564,

    /// <summary><c>Instruction_PirateNinja_SetFunction0FAC_Active</c> at $B2:F590.</summary>
    PirateNinja_SetFunction0FAC_Active = 0xf590,

    /// <summary><c>Instruction_PirateNinja_ResetSpeed</c> at $B2:F5D6.</summary>
    PirateNinja_ResetSpeed = 0xf5d6,

    /// <summary><c>Instruction_PirateNinja_SetLeftDivekickJumpInitialYSpeed</c> at $B2:F969.</summary>
    PirateNinja_SetLeftDivekickJumpInitialYSpeed = 0xf969,

    /// <summary><c>Instruction_PirateNinja_SetRightDivekickJumpInitialYSpeed</c> at $B2:FA3D.</summary>
    PirateNinja_SetRightDivekickJumpInitialYSpeed = 0xfa3d,

    /// <summary><c>Inst_PirateWall_MoveYPixelsDown_ChangeDirOnCollision_Left</c> at $B2:EE40.</summary>
    PirateWall_MoveYPixelsDown_ChangeDirOnCollision_Left = 0xee40,

    /// <summary><c>Inst_PirateWall_MoveYPixelsDown_ChangeDirOnCollision_Right</c> at $B2:EE72.</summary>
    PirateWall_MoveYPixelsDown_ChangeDirOnCollision_Right = 0xee72,

    /// <summary><c>Instruction_PirateWall_RandomlyChooseADirection_LeftWall</c> at $B2:EEA4.</summary>
    PirateWall_RandomlyChooseADirection_LeftWall = 0xeea4,

    /// <summary><c>Instruction_PirateWall_RandomlyChooseADirection_RightWall</c> at $B2:EEBC.</summary>
    PirateWall_RandomlyChooseADirection_RightWall = 0xeebc,

    /// <summary><c>Instruction_PirateWall_PrepareWallJumpToRight</c> at $B2:EED4.</summary>
    PirateWall_PrepareWallJumpToRight = 0xeed4,

    /// <summary><c>Instruction_PirateWall_PrepareWallJumpToLeft</c> at $B2:EEFD.</summary>
    PirateWall_PrepareWallJumpToLeft = 0xeefd,

    /// <summary><c>Instruction_PirateWall_FireLaserLeft</c> at $B2:EF2A.</summary>
    PirateWall_FireLaserLeft = 0xef2a,

    /// <summary><c>Instruction_PirateWall_FireLaserRight</c> at $B2:EF5D.</summary>
    PirateWall_FireLaserRight = 0xef5d,

    /// <summary><c>Instruction_PirateWall_QueueSpacePirateAttackSFX</c> at $B2:EF93.</summary>
    PirateWall_QueueSpacePirateAttackSFX = 0xef93,

    /// <summary><c>Instruction_PirateWalking_FireLaserLeftWithYOffsetInY</c> at $B2:FC68.</summary>
    PirateWalking_FireLaserLeftWithYOffsetInY = 0xfc68,

    /// <summary><c>Instruction_PirateWalking_FireLaserRightWithYOffsetInY</c> at $B2:FC90.</summary>
    PirateWalking_FireLaserRightWithYOffsetInY = 0xfc90,

    /// <summary><c>Instruction_PirateWalking_FunctionInY</c> at $B2:FCB8.</summary>
    PirateWalking_FunctionInY = 0xfcb8,

    /// <summary><c>Instruction_PirateWalking_ChooseAMovement</c> at $B2:FCC8.</summary>
    PirateWalking_ChooseAMovement = 0xfcc8,

}
