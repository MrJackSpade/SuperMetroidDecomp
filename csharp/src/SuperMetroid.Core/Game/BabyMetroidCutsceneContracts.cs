namespace SuperMetroid.Core.Game;

/// <summary>Named equivalents of the entrance's bank-$A9 function pointers.</summary>
public enum BabyMetroidCutscenePhase
{
    Inactive,
    DashOntoScreen,
    CurveTowardMotherBrainHead,
    GetRightUpInMotherBrainsFace,
    LatchOntoMotherBrain,
    SetMotherBrainToStumbleBack,
    ActivateRainbowBeamAndMotherBrainBody,
    WaitForMotherBrainToTurnToCorpse,
    StopDraining,
    LetGoAndSpawnDustClouds,
    MoveToTheCeiling,
    MoveToSamus,
    LatchOntoSamus,
    HealSamusToFullHealth,
    IdleUntilNoHealth,
    ReleaseSamus,
    StareDownMotherBrain,
    FlyOffScreen,
    MoveToFinalChargeStart,
    InitiateFinalCharge,
    FinalCharge,
    TakeFinalBlow,
    PlaySamusTheme,
    PrepareSamusForHyperBeam,
    DeathSequence,
    UnloadTiles,
    LetSamusRainbowSomeMore,
    FinalCutscene,
}

/// <summary>Named equivalents of `$A9:CD30/$CD4B`'s indirect palette functions.</summary>
public enum BabyMetroidSamusRainbowPhase
{
    Inactive,
    ActivateWhenEnemyIsLow,
    GraduallySlowAnimationDown,
}

/// <summary>Health/flash mutation produced by one colliding Mother Brain blue ring.</summary>
public readonly record struct BabyMetroidOnionRingHitResult(
    bool Applied,
    ushort HealthBefore,
    ushort HealthAfter,
    ushort FlashTimer);

/// <summary>Whole/subpixel coordinates before or after one cutscene-enemy main-AI call.</summary>
public readonly record struct BabyMetroidCutscenePoint(
    ushort XPosition,
    ushort XSubposition,
    ushort YPosition,
    ushort YSubposition);

/// <summary>One `$86:E509` dust explosion requested during the Baby's death.</summary>
public readonly record struct BabyMetroidDeathExplosionRequest(
    ushort PatternIndex,
    ushort XPosition,
    ushort YPosition,
    ushort ProjectileParameter,
    ushort SoundEffect);

/// <summary>
/// One of the three parameter-nine `$86:E509` dust clouds emitted by
/// <c>$A9:C98C-C9C2</c> when the Baby releases Mother Brain's head.
/// </summary>
public readonly record struct BabyMetroidReleaseDustRequest(
    ushort XPosition,
    ushort YPosition,
    ushort ProjectileParameter);

/// <summary>One fourteen-colour write from `$AD:E90C-$E998` to sprite palette seven.</summary>
public readonly record struct BabyMetroidPaletteTransferRequest(
    ushort PaletteIndex,
    uint SourceAddress,
    ushort DestinationColorIndex,
    ushort ColorCount);

/// <summary>One phase-three room-light palette pair selected by `$AD:F24B`.</summary>
public readonly record struct MotherBrainBackgroundPaletteTransferRequest(
    ushort PaletteIndex,
    uint SourceAddress,
    ushort FirstDestinationColorIndex,
    ushort SecondDestinationColorIndex,
    ushort ColorsPerDestination);

/// <summary>One-call debugger witness for the Baby entrance and latch chain.</summary>
public readonly record struct BabyMetroidCutsceneStepResult(
    BabyMetroidCutscenePhase PhaseBefore,
    BabyMetroidCutscenePhase PhaseAfter,
    BabyMetroidCutscenePoint Before,
    BabyMetroidCutscenePoint After,
    ushort XVelocity,
    ushort YVelocity,
    ushort Speed,
    ushort Angle,
    ushort FunctionTimer,
    bool BrainCollision,
    bool SamusStandingRequested,
    bool BodyStumbleRequested,
    bool MotherBrainInterrupted,
    bool LatchSoundQueued,
    ushort InstructionList,
    IReadOnlyList<BabyMetroidReleaseDustRequest> ReleaseDustClouds,
    bool SamusCrouchingRequested,
    ushort MovementTablePointer,
    bool AmbientCrySoundQueued,
    bool SamusTouchCollision,
    bool HealingCompleted,
    ushort Health,
    bool SamusRainbowActivated,
    bool SamusAnimationFrozen,
    bool SamusRainbowDisabled,
    bool HyperBeamEnabled,
    bool PhaseThreeHandoff,
    BabyMetroidDeathExplosionRequest? DeathExplosion,
    BabyMetroidPaletteTransferRequest? BabyPaletteTransfer,
    MotherBrainSpriteTileTransferRequest? AttackTileTransfer,
    MotherBrainBackgroundPaletteTransferRequest? BackgroundPaletteTransfer);
