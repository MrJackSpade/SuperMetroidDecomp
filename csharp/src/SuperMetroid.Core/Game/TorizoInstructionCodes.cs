namespace SuperMetroid.Core.Game;

/// <summary>Named bank-$AA instruction entry points used by Bomb and Golden Torizo bytecode.</summary>
internal static class TorizoInstructionCodes
{
    /// <summary><c>Instruction_CommonAA_Enemy0FB2_InY</c> at $AA:806B.</summary>
    public const ushort Instruction_CommonAA_Enemy0FB2_InY = 0x806b;

    /// <summary><c>Instruction_CommonAA3_SetEnemy0FB2ToRTS</c> at $AA:8074.</summary>
    public const ushort Instruction_CommonAA3_SetEnemy0FB2ToRTS = 0x8074;

    /// <summary><c>Instruction_Torizo_FunctionInY</c> at $AA:B09C.</summary>
    public const ushort Instruction_Torizo_FunctionInY = 0xb09c;

    /// <summary><c>Instruction_Torizo_MarkBTGutBlownUp_Spawn6BTDroolProjectiles</c> at $AA:B11D.</summary>
    public const ushort Instruction_Torizo_MarkBTGutBlownUp_Spawn6BTDroolProjectiles = 0xb11d;

    /// <summary><c>Instruction_Torizo_MarkBombTorizoFaceBlownUp</c> at $AA:B1BE.</summary>
    public const ushort Instruction_Torizo_MarkBombTorizoFaceBlownUp = 0xb1be;

    /// <summary><c>Instruction_Torizo_SetAsVisible</c> at $AA:B224.</summary>
    public const ushort Instruction_Torizo_SetAsVisible = 0xb224;

    /// <summary><c>Instruction_Torizo_SetAsInvisible</c> at $AA:B22E.</summary>
    public const ushort Instruction_Torizo_SetAsInvisible = 0xb22e;

    /// <summary><c>Instruction_Torizo_SetupPaletteTransitionToBlack</c> at $AA:B238.</summary>
    public const ushort Instruction_Torizo_SetupPaletteTransitionToBlack = 0xb238;

    /// <summary><c>Instruction_Torizo_SetBossBit_QueueElevatorMusic_SpawnDrops</c> at $AA:B24D.</summary>
    public const ushort Instruction_Torizo_SetBossBit_QueueElevatorMusic_SpawnDrops = 0xb24d;

    /// <summary><c>Instruction_Torizo_AdvanceGradualColorChange</c> at $AA:B271.</summary>
    public const ushort Instruction_Torizo_AdvanceGradualColorChange = 0xb271;

    /// <summary><c>Instruction_Torizo_SetupPaletteTransitionToNormalTorizo</c> at $AA:B94D.</summary>
    public const ushort Instruction_Torizo_SetupPaletteTransitionToNormalTorizo = 0xb94d;

    /// <summary><c>Instruction_Torizo_StartFightMusic_BombTorizoBellyPaletteFX</c> at $AA:B951.</summary>
    public const ushort Instruction_Torizo_StartFightMusic_BombTorizoBellyPaletteFX = 0xb951;

    /// <summary><c>RTL_AAC2C8</c> at $AA:C2C8.</summary>
    public const ushort RTL_AAC2C8 = 0xc2c8;

    /// <summary><c>Instruction_Torizo_SetAnimationLock</c> at $AA:C2C9.</summary>
    public const ushort Instruction_Torizo_SetAnimationLock = 0xc2c9;

    /// <summary><c>Instruction_Torizo_ClearAnimationLock</c> at $AA:C2D1.</summary>
    public const ushort Instruction_Torizo_ClearAnimationLock = 0xc2d1;

    /// <summary><c>Instruction_Torizo_GotoY_IfFaceBlownUp_ElseGotoY2_IfGolden</c> at $AA:C2D9.</summary>
    public const ushort Instruction_Torizo_GotoY_IfFaceBlownUp_ElseGotoY2_IfGolden = 0xc2d9;

    /// <summary><c>Instruction_Torizo_LinkInstructionInY</c> at $AA:C2ED.</summary>
    public const ushort Instruction_Torizo_LinkInstructionInY = 0xc2ed;

    /// <summary><c>Instruction_Torizo_Return</c> at $AA:C2F7.</summary>
    public const ushort Instruction_Torizo_Return = 0xc2f7;

    /// <summary><c>Instruction_Torizo_GotoGutExplosionLinkInstruction</c> at $AA:C2FD.</summary>
    public const ushort Instruction_Torizo_GotoGutExplosionLinkInstruction = 0xc2fd;

    /// <summary><c>Instruction_Torizo_Spawn5LowHealthExplosion_SleepFor28Frames</c> at $AA:C303.</summary>
    public const ushort Instruction_Torizo_Spawn5LowHealthExplosion_SleepFor28Frames = 0xc303;

    /// <summary><c>Instruction_Torizo_SpawnTorizoDeathExplosion_SleepFor1IFrame</c> at $AA:C32F.</summary>
    public const ushort Instruction_Torizo_SpawnTorizoDeathExplosion_SleepFor1IFrame = 0xc32f;

    /// <summary><c>Instruction_Torizo_SpawnTorizoLandingDustClouds</c> at $AA:C34A.</summary>
    public const ushort Instruction_Torizo_SpawnTorizoLandingDustClouds = 0xc34a;

    /// <summary><c>Instruction_Torizo_SpawnLowHealthInitialDroolIfHealthIsLow</c> at $AA:C35B.</summary>
    public const ushort Instruction_Torizo_SpawnLowHealthInitialDroolIfHealthIsLow = 0xc35b;

    /// <summary><c>Instruction_Torizo_SetTorizoTurningAroundFlag</c> at $AA:C36D.</summary>
    public const ushort Instruction_Torizo_SetTorizoTurningAroundFlag = 0xc36d;

    /// <summary><c>Instruction_Torizo_SetSteppedLeftWithLeftFootState</c> at $AA:C377.</summary>
    public const ushort Instruction_Torizo_SetSteppedLeftWithLeftFootState = 0xc377;

    /// <summary><c>Instruction_Torizo_SetSteppedRightWithRightFootState</c> at $AA:C38A.</summary>
    public const ushort Instruction_Torizo_SetSteppedRightWithRightFootState = 0xc38a;

    /// <summary><c>Instruction_Torizo_SetSteppedLeftWithRightFootState</c> at $AA:C3A0.</summary>
    public const ushort Instruction_Torizo_SetSteppedLeftWithRightFootState = 0xc3a0;

    /// <summary><c>Instruction_Torizo_SetSteppedRightWithLeftFootState</c> at $AA:C3B6.</summary>
    public const ushort Instruction_Torizo_SetSteppedRightWithLeftFootState = 0xc3b6;

    /// <summary><c>Instruction_Torizo_StandingUpMovement_IndexInY</c> at $AA:C3CC.</summary>
    public const ushort Instruction_Torizo_StandingUpMovement_IndexInY = 0xc3cc;

    /// <summary><c>Instruction_Torizo_SittingDownMovement_IndexInY</c> at $AA:C41E.</summary>
    public const ushort Instruction_Torizo_SittingDownMovement_IndexInY = 0xc41e;

    /// <summary><c>Instruction_Torizo_BombTorizoWalkingMovement_Normal_IndexInY</c> at $AA:C470.</summary>
    public const ushort Instruction_Torizo_BombTorizoWalkingMovement_Normal_IndexInY = 0xc470;

    /// <summary><c>Instruction_Torizo_BTWalkingMovement_Faceless_IndexInY</c> at $AA:C4E5.</summary>
    public const ushort Instruction_Torizo_BTWalkingMovement_Faceless_IndexInY = 0xc4e5;

    /// <summary><c>Instruction_Torizo_GotoY_IfRising</c> at $AA:C55A.</summary>
    public const ushort Instruction_Torizo_GotoY_IfRising = 0xc55a;

    /// <summary><c>Instruction_Torizo_CallYIfSamusIsLessThan38PixelsInFront</c> at $AA:C567.</summary>
    public const ushort Instruction_Torizo_CallYIfSamusIsLessThan38PixelsInFront = 0xc567;

    /// <summary><c>Instruction_Torizo_GotoYAndJumpBackwardsIfLessThan20Pixels</c> at $AA:C58B.</summary>
    public const ushort Instruction_Torizo_GotoYAndJumpBackwardsIfLessThan20Pixels = 0xc58b;

    /// <summary><c>Instruction_Torizo_CallY_OrY2_ForBombTorizoAttack</c> at $AA:C5A4.</summary>
    public const ushort Instruction_Torizo_CallY_OrY2_ForBombTorizoAttack = 0xc5a4;

    /// <summary><c>Instruction_Torizo_SpawnBombTorizosChozoOrbs</c> at $AA:C5CB.</summary>
    public const ushort Instruction_Torizo_SpawnBombTorizosChozoOrbs = 0xc5cb;

    /// <summary><c>Instruction_Torizo_SpawnBombTorizoSonicBoomWithParameterY</c> at $AA:C5E3.</summary>
    public const ushort Instruction_Torizo_SpawnBombTorizoSonicBoomWithParameterY = 0xc5e3;

    /// <summary><c>Instruction_Torizo_SpawnGoldenTorizoSonicBoomWithParameterY</c> at $AA:C5F2.</summary>
    public const ushort Instruction_Torizo_SpawnGoldenTorizoSonicBoomWithParameterY = 0xc5f2;

    /// <summary><c>Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY</c> at $AA:C601.</summary>
    public const ushort Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY = 0xc601;

    /// <summary><c>Instruction_Torizo_PlayShotTorizoSFX</c> at $AA:C610.</summary>
    public const ushort Instruction_Torizo_PlayShotTorizoSFX = 0xc610;

    /// <summary><c>Instruction_Torizo_PlayTorizoFootstepsSFX</c> at $AA:C618.</summary>
    public const ushort Instruction_Torizo_PlayTorizoFootstepsSFX = 0xc618;

    /// <summary><c>Instruction_Torizo_GotoY_IfNotHitGround</c> at $AA:CACE.</summary>
    public const ushort Instruction_Torizo_GotoY_IfNotHitGround = 0xcace;

    /// <summary><c>Instruction_Torizo_LoadGoldenTorizoPalettes</c> at $AA:CADE.</summary>
    public const ushort Instruction_Torizo_LoadGoldenTorizoPalettes = 0xcade;

    /// <summary><c>Inst_Torizo_StartFightMusic_GoldenTorizoBellyPaletteFX</c> at $AA:CAE2.</summary>
    public const ushort Inst_Torizo_StartFightMusic_GoldenTorizoBellyPaletteFX = 0xcae2;

    /// <summary><c>Instruction_GoldenTorizo_ClearCaughtSuperMissileFlag</c> at $AA:CDD7.</summary>
    public const ushort Instruction_GoldenTorizo_ClearCaughtSuperMissileFlag = 0xcdd7;

    /// <summary><c>Instruction_GoldenTorizo_SpawnGoldenTorizoEgg</c> at $AA:D0E9.</summary>
    public const ushort Instruction_GoldenTorizo_SpawnGoldenTorizoEgg = 0xd0e9;

    /// <summary><c>Instruction_GoldenTorizo_EyeBeamAttack_0</c> at $AA:D0F3.</summary>
    public const ushort Instruction_GoldenTorizo_EyeBeamAttack_0 = 0xd0f3;

    /// <summary><c>Instruction_GoldenTorizo_DisableEyeBeamExplosions</c> at $AA:D17B.</summary>
    public const ushort Instruction_GoldenTorizo_DisableEyeBeamExplosions = 0xd17b;

    /// <summary><c>Instruction_GoldenTorizo_EnableEyeBeamExplosions</c> at $AA:D187.</summary>
    public const ushort Instruction_GoldenTorizo_EnableEyeBeamExplosions = 0xd187;

    /// <summary><c>Instruction_GoldenTorizo_UnmarkStunned</c> at $AA:D1E7.</summary>
    public const ushort Instruction_GoldenTorizo_UnmarkStunned = 0xd1e7;

    /// <summary><c>Instruction_GoldenTorizo_QueueEggReleasedSFX</c> at $AA:D38F.</summary>
    public const ushort Instruction_GoldenTorizo_QueueEggReleasedSFX = 0xd38f;

    /// <summary><c>Instruction_GoldenTorizo_QueueLaserSFX</c> at $AA:D397.</summary>
    public const ushort Instruction_GoldenTorizo_QueueLaserSFX = 0xd397;

    /// <summary><c>Instruction_Torizo_QueueSonicBoomSFX</c> at $AA:D39F.</summary>
    public const ushort Instruction_Torizo_QueueSonicBoomSFX = 0xd39f;

    /// <summary><c>Instruction_GoldenTorizo_SpawnSuperMissile</c> at $AA:D3E0.</summary>
    public const ushort Instruction_GoldenTorizo_SpawnSuperMissile = 0xd3e0;

    /// <summary><c>Instruction_GoldenTorizo_GotoY_IfSamusIsMorphedBehindTorizo</c> at $AA:D3EA.</summary>
    public const ushort Instruction_GoldenTorizo_GotoY_IfSamusIsMorphedBehindTorizo = 0xd3ea;

    /// <summary><c>Instruction_GoldenTorizo_SpawnEyeBeam</c> at $AA:D436.</summary>
    public const ushort Instruction_GoldenTorizo_SpawnEyeBeam = 0xd436;

    /// <summary><c>Instruction_GT_CallY_25Chance_IfSamusMorphedInFrontOfTorizo</c> at $AA:D445.</summary>
    public const ushort Instruction_GT_CallY_25Chance_IfSamusMorphedInFrontOfTorizo = 0xd445;

    /// <summary><c>Instruction_GoldenTorizo_CallY_25Chance_IfHealthLessThan789</c> at $AA:D474.</summary>
    public const ushort Instruction_GoldenTorizo_CallY_25Chance_IfHealthLessThan789 = 0xd474;

    /// <summary><c>Instruction_GoldenTorizo_CallY_IfStunHealthGreaterThan2A31</c> at $AA:D49B.</summary>
    public const ushort Instruction_GoldenTorizo_CallY_IfStunHealthGreaterThan2A31 = 0xd49b;

    /// <summary><c>Instruction_GoldenTorizo_GotoY_JumpForwards_IfAtLeast70Pixel</c> at $AA:D4BA.</summary>
    public const ushort Instruction_GoldenTorizo_GotoY_JumpForwards_IfAtLeast70Pixel = 0xd4ba;

    /// <summary><c>Instruction_GoldenTorizo_SpawnChozoOrbs</c> at $AA:D4F3.</summary>
    public const ushort Instruction_GoldenTorizo_SpawnChozoOrbs = 0xd4f3;

    /// <summary><c>Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels</c> at $AA:D4FD.</summary>
    public const ushort Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels = 0xd4fd;

    /// <summary><c>Instruction_GoldenTorizo_CallY_OrY2_ForAttack</c> at $AA:D526.</summary>
    public const ushort Instruction_GoldenTorizo_CallY_OrY2_ForAttack = 0xd526;

    /// <summary><c>Instruction_GoldenTorizo_WalkingMovement_IndexInY</c> at $AA:D54D.</summary>
    public const ushort Instruction_GoldenTorizo_WalkingMovement_IndexInY = 0xd54d;

}
