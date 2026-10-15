namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>What a cue of Torizo's body programs does: one per bank-$AA Torizo instruction its lists use.</summary>
internal enum TorizoCueKind
{
    /// <summary>Installs the body's pre-instruction (<c>Instruction_CommonAA_Enemy0FB2_InY</c>, $AA:806B).</summary>
    SetPreInstruction,
    /// <summary>Clears the body's pre-instruction (<c>Instruction_CommonAA3_SetEnemy0FB2ToRTS</c>, $AA:8074).</summary>
    ClearPreInstruction,
    /// <summary>Selects the main-AI function (<c>Instruction_Torizo_FunctionInY</c>, $AA:B09C).</summary>
    SetFunction,
    /// <summary>Marks Bomb Torizo's gut blown up and spawns six drool pieces (<c>Instruction_Torizo_MarkBTGutBlownUp_Spawn6BTDroolProjectiles</c>, $AA:B11D).</summary>
    BlowUpGut,
    /// <summary>Marks Bomb Torizo's face blown up (<c>Instruction_Torizo_MarkBombTorizoFaceBlownUp</c>, $AA:B1BE).</summary>
    MarkFaceBlownUp,
    /// <summary>Makes the body visible (<c>Instruction_Torizo_SetAsVisible</c>, $AA:B224).</summary>
    MakeVisible,
    /// <summary>Makes the body invisible (<c>Instruction_Torizo_SetAsInvisible</c>, $AA:B22E).</summary>
    MakeInvisible,
    /// <summary>Starts the palette fade to black (<c>Instruction_Torizo_SetupPaletteTransitionToBlack</c>, $AA:B238).</summary>
    FadeToBlack,
    /// <summary>Sets the boss bit, queues the elevator music and spawns the drops (<c>Instruction_Torizo_SetBossBit_QueueElevatorMusic_SpawnDrops</c>, $AA:B24D).</summary>
    FinishDeath,
    /// <summary>Advances the gradual palette fade (<c>Instruction_Torizo_AdvanceGradualColorChange</c>, $AA:B271).</summary>
    AdvancePaletteFade,
    /// <summary>Starts the palette fade to the normal colors (<c>Instruction_Torizo_SetupPaletteTransitionToNormalTorizo</c>, $AA:B94D).</summary>
    FadeToNormalColors,
    /// <summary>Starts Bomb Torizo's fight music and belly palette effect (<c>Instruction_Torizo_StartFightMusic_BombTorizoBellyPaletteFX</c>, $AA:B951).</summary>
    StartBombTorizoFight,
    /// <summary>Does nothing (<c>RTL_AAC2C8</c>, $AA:C2C8).</summary>
    NoOperation,
    /// <summary>Sets the animation lock (<c>Instruction_Torizo_SetAnimationLock</c>, $AA:C2C9).</summary>
    LockAnimation,
    /// <summary>Clears the animation lock (<c>Instruction_Torizo_ClearAnimationLock</c>, $AA:C2D1).</summary>
    UnlockAnimation,
    /// <summary>Continues at the target when Bomb Torizo's face is blown up, else at the alternate for Golden Torizo (<c>Instruction_Torizo_GotoY_IfFaceBlownUp_ElseGotoY2_IfGolden</c>, $AA:C2D9).</summary>
    GoToIfFaceBlownUpElseIfGolden,
    /// <summary>Saves the target as the return point (<c>Instruction_Torizo_LinkInstructionInY</c>, $AA:C2ED).</summary>
    Link,
    /// <summary>Resumes at the saved return point (<c>Instruction_Torizo_Return</c>, $AA:C2F7).</summary>
    Return,
    /// <summary>Resumes the program the gut explosion interrupted (<c>Instruction_Torizo_GotoGutExplosionLinkInstruction</c>, $AA:C2FD).</summary>
    ResumeInterrupted,
    /// <summary>Spawns the low-health explosions and flashes; the program then waits forty updates (<c>Instruction_Torizo_Spawn5LowHealthExplosion_SleepFor28Frames</c>, $AA:C303).</summary>
    SpawnLowHealthExplosions,
    /// <summary>Spawns a death explosion; the program then waits one update (<c>Instruction_Torizo_SpawnTorizoDeathExplosion_SleepFor1IFrame</c>, $AA:C32F).</summary>
    SpawnDeathExplosion,
    /// <summary>Spawns the landing dust clouds (<c>Instruction_Torizo_SpawnTorizoLandingDustClouds</c>, $AA:C34A).</summary>
    SpawnLandingDust,
    /// <summary>Spawns drool when health is low (<c>Instruction_Torizo_SpawnLowHealthInitialDroolIfHealthIsLow</c>, $AA:C35B).</summary>
    SpawnLowHealthDrool,
    /// <summary>Marks the body turning around (<c>Instruction_Torizo_SetTorizoTurningAroundFlag</c>, $AA:C36D).</summary>
    SetTurningAround,
    /// <summary>Records a left step on the left foot (<c>Instruction_Torizo_SetSteppedLeftWithLeftFootState</c>, $AA:C377).</summary>
    SteppedLeftWithLeftFoot,
    /// <summary>Records a right step on the right foot (<c>Instruction_Torizo_SetSteppedRightWithRightFootState</c>, $AA:C38A).</summary>
    SteppedRightWithRightFoot,
    /// <summary>Records a left step on the right foot (<c>Instruction_Torizo_SetSteppedLeftWithRightFootState</c>, $AA:C3A0).</summary>
    SteppedLeftWithRightFoot,
    /// <summary>Records a right step on the left foot (<c>Instruction_Torizo_SetSteppedRightWithLeftFootState</c>, $AA:C3B6).</summary>
    SteppedRightWithLeftFoot,
    /// <summary>Applies a standing-up movement step (<c>Instruction_Torizo_StandingUpMovement_IndexInY</c>, $AA:C3CC).</summary>
    StandUp,
    /// <summary>Applies a sitting-down movement step (<c>Instruction_Torizo_SittingDownMovement_IndexInY</c>, $AA:C41E).</summary>
    SitDown,
    /// <summary>Takes a Bomb Torizo walking step, turning at a wall (<c>Instruction_Torizo_BombTorizoWalkingMovement_Normal_IndexInY</c>, $AA:C470).</summary>
    WalkBombTorizo,
    /// <summary>Takes a faceless Bomb Torizo walking step, turning at a wall (<c>Instruction_Torizo_BTWalkingMovement_Faceless_IndexInY</c>, $AA:C4E5).</summary>
    WalkFacelessBombTorizo,
    /// <summary>Continues at the target while rising (<c>Instruction_Torizo_GotoY_IfRising</c>, $AA:C55A).</summary>
    GoToIfRising,
    /// <summary>Calls the target when Samus is close behind (<c>Instruction_Torizo_CallYIfSamusIsLessThan38PixelsInFront</c>, $AA:C567).</summary>
    CallIfSamusCloseBehind,
    /// <summary>Jumps back to the target when Samus is close in front (<c>Instruction_Torizo_GotoYAndJumpBackwardsIfLessThan20Pixels</c>, $AA:C58B).</summary>
    JumpBackIfSamusCloseInFront,
    /// <summary>Calls one of Bomb Torizo's two attacks (<c>Instruction_Torizo_CallY_OrY2_ForBombTorizoAttack</c>, $AA:C5A4).</summary>
    CallBombTorizoAttack,
    /// <summary>Spawns Bomb Torizo's Chozo orbs (<c>Instruction_Torizo_SpawnBombTorizosChozoOrbs</c>, $AA:C5CB).</summary>
    SpawnBombTorizoChozoOrbs,
    /// <summary>Spawns a Bomb Torizo sonic boom (<c>Instruction_Torizo_SpawnBombTorizoSonicBoomWithParameterY</c>, $AA:C5E3).</summary>
    SpawnBombTorizoSonicBoom,
    /// <summary>Spawns a Golden Torizo sonic boom (<c>Instruction_Torizo_SpawnGoldenTorizoSonicBoomWithParameterY</c>, $AA:C5F2).</summary>
    SpawnGoldenTorizoSonicBoom,
    /// <summary>Spawns an explosive swipe (<c>Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY</c>, $AA:C601).</summary>
    SpawnExplosiveSwipe,
    /// <summary>Queues the shot sound (<c>Instruction_Torizo_PlayShotTorizoSFX</c>, $AA:C610).</summary>
    PlayShotSound,
    /// <summary>Queues the footstep sound (<c>Instruction_Torizo_PlayTorizoFootstepsSFX</c>, $AA:C618).</summary>
    PlayFootstepSound,
    /// <summary>Continues at the target until the body reaches the ground (<c>Instruction_Torizo_GotoY_IfNotHitGround</c>, $AA:CACE).</summary>
    GoToIfAirborne,
    /// <summary>Loads Golden Torizo's palettes (<c>Instruction_Torizo_LoadGoldenTorizoPalettes</c>, $AA:CADE).</summary>
    LoadGoldenTorizoPalettes,
    /// <summary>Starts Golden Torizo's fight music and belly palette effect (<c>Inst_Torizo_StartFightMusic_GoldenTorizoBellyPaletteFX</c>, $AA:CAE2).</summary>
    StartGoldenTorizoFight,
    /// <summary>Clears the caught-super-missile flag (<c>Instruction_GoldenTorizo_ClearCaughtSuperMissileFlag</c>, $AA:CDD7).</summary>
    ClearCaughtSuperMissile,
    /// <summary>Spawns a Golden Torizo egg (<c>Instruction_GoldenTorizo_SpawnGoldenTorizoEgg</c>, $AA:D0E9).</summary>
    SpawnEgg,
    /// <summary>Continues at the target while an eye beam is active (<c>Instruction_GoldenTorizo_EyeBeamAttack_0</c>, $AA:D0F3).</summary>
    GoToIfEyeBeamActive,
    /// <summary>Disables eye-beam explosions (<c>Instruction_GoldenTorizo_DisableEyeBeamExplosions</c>, $AA:D17B).</summary>
    DisableEyeBeamExplosions,
    /// <summary>Enables eye-beam explosions (<c>Instruction_GoldenTorizo_EnableEyeBeamExplosions</c>, $AA:D187).</summary>
    EnableEyeBeamExplosions,
    /// <summary>Clears the stunned mark (<c>Instruction_GoldenTorizo_UnmarkStunned</c>, $AA:D1E7).</summary>
    UnmarkStunned,
    /// <summary>Queues the egg-released sound (<c>Instruction_GoldenTorizo_QueueEggReleasedSFX</c>, $AA:D38F).</summary>
    PlayEggReleasedSound,
    /// <summary>Queues the laser sound (<c>Instruction_GoldenTorizo_QueueLaserSFX</c>, $AA:D397).</summary>
    PlayLaserSound,
    /// <summary>Queues the sonic-boom sound (<c>Instruction_Torizo_QueueSonicBoomSFX</c>, $AA:D39F).</summary>
    PlaySonicBoomSound,
    /// <summary>Spawns a super missile (<c>Instruction_GoldenTorizo_SpawnSuperMissile</c>, $AA:D3E0).</summary>
    SpawnSuperMissile,
    /// <summary>Continues at the target when Samus is morphed behind the body (<c>Instruction_GoldenTorizo_GotoY_IfSamusIsMorphedBehindTorizo</c>, $AA:D3EA).</summary>
    GoToIfSamusMorphedBehind,
    /// <summary>Spawns an eye beam (<c>Instruction_GoldenTorizo_SpawnEyeBeam</c>, $AA:D436).</summary>
    SpawnEyeBeam,
    /// <summary>Calls the target, at random, when Samus is morphed in front (<c>Instruction_GT_CallY_25Chance_IfSamusMorphedInFrontOfTorizo</c>, $AA:D445).</summary>
    CallSometimesIfSamusMorphedInFront,
    /// <summary>Calls the target, at random, when health is low (<c>Instruction_GoldenTorizo_CallY_25Chance_IfHealthLessThan789</c>, $AA:D474).</summary>
    CallSometimesIfHealthLow,
    /// <summary>Calls the target when the stun health is high (<c>Instruction_GoldenTorizo_CallY_IfStunHealthGreaterThan2A31</c>, $AA:D49B).</summary>
    CallIfStunHealthHigh,
    /// <summary>Jumps forward to the target when Samus is far (<c>Instruction_GoldenTorizo_GotoY_JumpForwards_IfAtLeast70Pixel</c>, $AA:D4BA).</summary>
    JumpForwardIfSamusFar,
    /// <summary>Spawns Golden Torizo's Chozo orbs (<c>Instruction_GoldenTorizo_SpawnChozoOrbs</c>, $AA:D4F3).</summary>
    SpawnGoldenTorizoChozoOrbs,
    /// <summary>Jumps back to the target when Samus is close in front of Golden Torizo (<c>Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels</c>, $AA:D4FD).</summary>
    JumpBackIfSamusCloseInFrontGolden,
    /// <summary>Calls one of Golden Torizo's two attacks (<c>Instruction_GoldenTorizo_CallY_OrY2_ForAttack</c>, $AA:D526).</summary>
    CallGoldenTorizoAttack,
    /// <summary>Takes a Golden Torizo walking step, turning at a wall (<c>Instruction_GoldenTorizo_WalkingMovement_IndexInY</c>, $AA:D54D).</summary>
    WalkGoldenTorizo,
    /// <summary>Copies a tile transfer to VRAM (the common <c>$814B</c> transfer).</summary>
    CopyTiles,
}
