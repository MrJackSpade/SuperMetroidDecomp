namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>What a cue of Torizo's body programs does: one per bank-$AA Torizo instruction its lists use.</summary>
internal enum TorizoCueKind
{
    /// <summary>Clears the body's pre-instruction (<c>Instruction_CommonAA3_SetEnemy0FB2ToRTS</c>, $AA:8074).</summary>
    ClearPreInstruction,
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
    /// <summary>Resumes at the saved return point (<c>Instruction_Torizo_Return</c>, $AA:C2F7).</summary>
    Return,
    /// <summary>Resumes the program the gut explosion interrupted (<c>Instruction_Torizo_GotoGutExplosionLinkInstruction</c>, $AA:C2FD).</summary>
    ResumeInterrupted,
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
    /// <summary>Spawns Bomb Torizo's Chozo orbs (<c>Instruction_Torizo_SpawnBombTorizosChozoOrbs</c>, $AA:C5CB).</summary>
    SpawnBombTorizoChozoOrbs,
    /// <summary>Queues the shot sound (<c>Instruction_Torizo_PlayShotTorizoSFX</c>, $AA:C610).</summary>
    PlayShotSound,
    /// <summary>Queues the footstep sound (<c>Instruction_Torizo_PlayTorizoFootstepsSFX</c>, $AA:C618).</summary>
    PlayFootstepSound,
    /// <summary>Loads Golden Torizo's palettes (<c>Instruction_Torizo_LoadGoldenTorizoPalettes</c>, $AA:CADE).</summary>
    LoadGoldenTorizoPalettes,
    /// <summary>Starts Golden Torizo's fight music and belly palette effect (<c>Inst_Torizo_StartFightMusic_GoldenTorizoBellyPaletteFX</c>, $AA:CAE2).</summary>
    StartGoldenTorizoFight,
    /// <summary>Clears the caught-super-missile flag (<c>Instruction_GoldenTorizo_ClearCaughtSuperMissileFlag</c>, $AA:CDD7).</summary>
    ClearCaughtSuperMissile,
    /// <summary>Spawns a Golden Torizo egg (<c>Instruction_GoldenTorizo_SpawnGoldenTorizoEgg</c>, $AA:D0E9).</summary>
    SpawnEgg,
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
    /// <summary>Spawns Golden Torizo's Chozo orbs (<c>Instruction_GoldenTorizo_SpawnChozoOrbs</c>, $AA:D4F3).</summary>
    SpawnGoldenTorizoChozoOrbs,
}
