namespace SuperMetroid.Core.Game;

/// <summary>Exact active-attack function-pointer phases admitted by the sequence.</summary>
public enum MotherBrainRainbowBeamAttackPhase
{
    Inactive,
    StartCharging,
    RetractNeck,
    WaitForCharge,
    ExtendNeckDown,
    StartFiring,
    MoveSamusTowardWall,
    OneFrameDelay,
    StartDrainingSamus,
    DrainingSamus,
    FinishFiring,
    LetSamusFall,
    WaitForSamusToLand,
    LowerHead,
    DecideNextAction,
    RepeatAttack,
    FinishSamusOff,
    FinishStandUp,
    AdmireJobWellDone,
    ChargeFinalRainbowBeam,
    LoadBabyMetroidTiles,
    FireFinalRainbowBeam,
    FinalRainbowBeamHolding,
    DrainedByBabyMetroidTakenAback,
    DrainedByBabyMetroidRegainBalance,
    DrainedByBabyMetroidFiringRainbowBeam,
    DrainedByBabyMetroidRainbowBeamRunOut,
    DrainedByBabyMetroidMoveToBackOfRoom,
    DrainedByBabyMetroidGoIntoLowPowerMode,
    DrainedByBabyMetroidPrepareTransitionToGrey,
    DrainedByBabyMetroidTransitionToGrey,
    Phase2ReviveSelfInanimateGrey,
    Phase2ReviveSelfShowSignsOfLife,
    Phase2ReviveSelfTransitionFromGrey,
    Phase2ReviveSelfWakeUp,
    Phase2ReviveSelfWakeUpStretch,
    Phase2ReviveSelfWalkUpToBabyMetroid,
    Phase2ReviveSelfPrepareNeckForBabyMetroidDeath,
    Phase2ReviveSelfFinishPreparingForBabyMetroidDeath,
    Phase2MurderBabyMetroidAttack,
    Phase2MurderBabyMetroidAttackCooldown,
    PrepareForFinalBabyMetroidAttack,
    ExecuteFinalBabyMetroidAttack,
    FinalBabyMetroidAttackHolding,
    Phase3RecoverFromCutsceneMakeSomeDistance,
    Phase3RecoverFromCutsceneSetupForFighting,
    Phase3FightingMain,
    Phase3FightingAttackCooldown,
    Phase3DeathSequenceMoveToBackOfRoom,
    Phase3DeathSequenceIdleWhilstExploding,
    Phase3DeathSequenceStumbleToMiddleOfRoom,
    Phase3DeathSequenceDisableBrainEffects,
    Phase3DeathSequenceSetupBodyFadeOut,
    Phase3DeathSequenceFadeOutBody,
    Phase3DeathSequenceFinalFewExplosions,
    Phase3DeathSequenceRealizeDecapitation,
    Phase3DeathSequenceBrainFallsToGround,
    Phase3DeathSequenceLoadCorpseTiles,
    Phase3DeathSequenceSetupFadeToGrey,
    Phase3DeathSequenceFadeToGrey,
    Phase3DeathSequenceCorpseTipsOver,
    Phase3DeathSequenceCorpseRotsAway,
    Phase3DeathSequence20FrameDelay,
    Phase3DeathSequenceLoadEscapeTimerTiles,
    Phase3DeathSequenceStartEscape,
    Phase3DeathSequenceSpawnTimeBombSetSubtitle,
    Phase3DeathSequenceTypeOutZebesEscapeText,
    Phase3DeathSequenceDoorExplodingStartTimer,
    Phase3DeathSequenceBlowUpEscapeDoor,
    Phase3DeathSequenceKeepEarthquakeGoing,
}

/// <summary>Reachable third-phase walking function pointers at `$A9:C26A-$C326`.</summary>
public enum MotherBrainPhase3WalkingPhase
{
    Inactive,
    TryToInchForward,
    RetreatQuickly,
    RetreatSlowly,
}

/// <summary>Reachable third-phase neck function pointers at `$A9:C330-$C3EE`.</summary>
public enum MotherBrainPhase3NeckPhase
{
    Inactive,
    Normal,
    SetupRecoilRecovery,
    RecoilRecovery,
    SetupHyperBeamRecoil,
    HyperBeamRecoil,
}

/// <summary>Low-three-bit projectile classes consumed by `$A9:B58E`.</summary>
public enum MotherBrainProjectileType
{
    Beam = 0,
    Missile = 1,
    SuperMissile = 2,
}

/// <summary>
/// One native seven-byte sprite-tile transfer entry consumed by `$A9:C5BE`.
/// </summary>
public readonly record struct MotherBrainSpriteTileTransferRequest(
    ushort EntryIndex,
    ushort Size,
    uint SourceAddress,
    ushort VramDestination);

/// <summary>One requested beam explosion projectile and its native sound number.</summary>
public readonly record struct MotherBrainRainbowExplosionRequest(
    short XOffset,
    short YOffset);

/// <summary>One projectile in a simultaneous `$A9:B03E` death-explosion batch.</summary>
public readonly record struct MotherBrainDeathExplosionRequest(
    short XOffset,
    short YOffset,
    ushort ProjectileParameter,
    ushort SoundEffect);

/// <summary>One periodic misc-dust projectile emitted around the escape door by `$A9:B346`.</summary>
public readonly record struct MotherBrainEscapeDoorExplosionRequest(
    ushort XPosition,
    ushort YPosition,
    ushort ProjectileParameter,
    ushort SoundEffect);

/// <summary>One of eight `$86:CB21` door-fragment allocation attempts from `$A9:B3A3`.</summary>
public readonly record struct MotherBrainEscapeDoorParticleSpawnRequest(ushort Parameter);

/// <summary>The hardcoded room-object request made after the escape door fragments spawn.</summary>
public readonly record struct MotherBrainEscapeDoorPlmRequest(
    byte BlockX,
    byte BlockY,
    ushort PlmEntry);

/// <summary>Debugger witness for one Mother Brain active-rainbow body-function call.</summary>
public readonly record struct MotherBrainRainbowBeamAttackStepResult(
    MotherBrainRainbowBeamAttackPhase PhaseBefore,
    MotherBrainRainbowBeamAttackPhase PhaseAfter,
    MotherBrainForcedSamusMovementResult? Movement,
    bool SoundQueued,
    bool PaletteRequested,
    MotherBrainRainbowExplosionRequest? Explosion,
    bool ChargeSoundQueued,
    bool BodyWalkRequested,
    bool HeadInstructionListRequested,
    bool BodyPostureRequested,
    MotherBrainSpriteTileTransferRequest? SpriteTileTransfer,
    bool BabySpawnRequested,
    bool FinalBeamSoundQueued,
    IReadOnlyList<MotherBrainDeathExplosionRequest> DeathExplosions,
    IReadOnlyList<MotherBrainSpriteTileTransferRequest> CorpseRottingVramTransfers,
    IReadOnlyList<MotherBrainCorpseDustRequest> CorpseDustRequests,
    bool MusicStopQueued,
    bool EscapeMusicQueued,
    IReadOnlyList<MotherBrainSpriteTileTransferRequest> EscapeSequenceTileTransfers,
    bool ExplodedDoorPaletteRequested,
    bool EscapeMusicTrackQueued,
    IReadOnlyList<ushort> EscapePaletteFxRequests,
    bool EscapeTypewriterSetupRequested,
    bool TimeBombSetSubtitleSpawnRequested,
    MotherBrainEscapeDoorExplosionRequest? EscapeDoorExplosion,
    bool TimerHandlingEnableRequested,
    bool MotherBrainEscapeTimerStartRequested,
    bool MotherBrainBossBitRequested,
    bool ZebesTimebombEventRequested,
    IReadOnlyList<MotherBrainEscapeDoorParticleSpawnRequest> EscapeDoorParticleSpawns,
    MotherBrainEscapeDoorPlmRequest? EscapeDoorPlm,
    bool EarthquakeTimerRefreshed);
