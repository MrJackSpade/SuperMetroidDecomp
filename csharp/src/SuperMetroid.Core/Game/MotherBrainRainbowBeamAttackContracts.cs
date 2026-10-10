using SuperMetroid.Core.Rooms;
namespace SuperMetroid.Core.Game;

/// <summary>Exact active-attack function-pointer phases admitted by the sequence.</summary>
public enum MotherBrainRainbowBeamAttackPhase
{
    /// <summary>No rainbow-beam or post-beam cutscene body function is active.</summary>
    Inactive,
    /// <summary>Initializes the first rainbow-beam charge, palette, timers, and Samus-control lock.</summary>
    StartCharging,
    /// <summary>Runs the neck retraction needed to align Mother Brain's head for charging.</summary>
    RetractNeck,
    /// <summary>Holds the charge presentation until its native countdown permits neck extension.</summary>
    WaitForCharge,
    /// <summary>Extends the neck downward into the firing alignment over Samus.</summary>
    ExtendNeckDown,
    /// <summary>Starts the sustained beam, associated sound, and firing posture.</summary>
    StartFiring,
    /// <summary>Forces Samus horizontally toward the room wall while the beam remains connected.</summary>
    MoveSamusTowardWall,
    /// <summary>Preserves the native single-update gap between forced movement and energy drain.</summary>
    OneFrameDelay,
    /// <summary>Initializes the drained-Samus state and the beam's health-removal cadence.</summary>
    StartDrainingSamus,
    /// <summary>Removes Samus's energy while maintaining the pinned beam and palette effects.</summary>
    DrainingSamus,
    /// <summary>Stops the sustained beam and begins restoring Mother Brain's post-fire pose.</summary>
    FinishFiring,
    /// <summary>Releases forced Samus positioning so gravity can drop her to the floor.</summary>
    LetSamusFall,
    /// <summary>Waits for Samus's drained body to reach its grounded state.</summary>
    WaitForSamusToLand,
    /// <summary>Lowers Mother Brain's head after the completed drain.</summary>
    LowerHead,
    /// <summary>Chooses another ordinary beam or the finishing branch from Samus's remaining energy.</summary>
    DecideNextAction,
    /// <summary>Resets attack state to charge and fire another ordinary rainbow beam.</summary>
    RepeatAttack,
    /// <summary>Begins the scripted attempt to kill drained Samus once ordinary repeats are complete.</summary>
    FinishSamusOff,
    /// <summary>Completes Mother Brain's return to an upright posture after the draining attack.</summary>
    FinishStandUp,
    /// <summary>Holds Mother Brain's victorious pause before the final beam charge.</summary>
    AdmireJobWellDone,
    /// <summary>Charges the fatal rainbow beam that triggers the Baby Metroid intervention.</summary>
    ChargeFinalRainbowBeam,
    /// <summary>Requests the Baby Metroid sprite tiles before the intervention actor is created.</summary>
    LoadBabyMetroidTiles,
    /// <summary>Fires the final beam toward drained Samus and opens the intervention timing window.</summary>
    FireFinalRainbowBeam,
    /// <summary>Maintains the final beam until the Baby Metroid entrance sequence takes ownership.</summary>
    FinalRainbowBeamHolding,
    /// <summary>Begins Mother Brain's recoil when the Baby Metroid drains her unexpectedly.</summary>
    DrainedByBabyMetroidTakenAback,
    /// <summary>Restores balance after the initial drain recoil while the Baby remains attached.</summary>
    DrainedByBabyMetroidRegainBalance,
    /// <summary>Attempts to fire the rainbow beam while the Baby continues draining Mother Brain.</summary>
    DrainedByBabyMetroidFiringRainbowBeam,
    /// <summary>Runs the beam's power-loss transition as Mother Brain's energy is exhausted.</summary>
    DrainedByBabyMetroidRainbowBeamRunOut,
    /// <summary>Moves the weakened body toward the rear of the arena.</summary>
    DrainedByBabyMetroidMoveToBackOfRoom,
    /// <summary>Enters the low-power posture after reaching the rear position.</summary>
    DrainedByBabyMetroidGoIntoLowPowerMode,
    /// <summary>Prepares body palette state for the drained transition to grey.</summary>
    DrainedByBabyMetroidPrepareTransitionToGrey,
    /// <summary>Advances the drained-body palette fade until Mother Brain is fully grey.</summary>
    DrainedByBabyMetroidTransitionToGrey,
    /// <summary>Holds the phase-two body motionless and grey at the start of self-revival.</summary>
    Phase2ReviveSelfInanimateGrey,
    /// <summary>Introduces the first movement and palette signs that Mother Brain is reviving.</summary>
    Phase2ReviveSelfShowSignsOfLife,
    /// <summary>Restores body color from the drained grey palette during revival.</summary>
    Phase2ReviveSelfTransitionFromGrey,
    /// <summary>Returns the revived body to active posture and animation.</summary>
    Phase2ReviveSelfWakeUp,
    /// <summary>Runs the post-revival stretch before Mother Brain approaches the Baby.</summary>
    Phase2ReviveSelfWakeUpStretch,
    /// <summary>Walks the revived body toward the Baby Metroid's held position.</summary>
    Phase2ReviveSelfWalkUpToBabyMetroid,
    /// <summary>Begins aligning the neck and head for the Baby Metroid killing attack.</summary>
    Phase2ReviveSelfPrepareNeckForBabyMetroidDeath,
    /// <summary>Completes neck alignment and waits for the murder-beam trigger.</summary>
    Phase2ReviveSelfFinishPreparingForBabyMetroidDeath,
    /// <summary>Fires the phase-two rainbow attack that damages the Baby Metroid.</summary>
    Phase2MurderBabyMetroidAttack,
    /// <summary>Waits between murder-beam volleys while the Baby cutscene advances.</summary>
    Phase2MurderBabyMetroidAttackCooldown,
    /// <summary>Prepares the final volley after the Baby Metroid returns for its charge.</summary>
    PrepareForFinalBabyMetroidAttack,
    /// <summary>Fires the killing beam during the Baby Metroid's final charge.</summary>
    ExecuteFinalBabyMetroidAttack,
    /// <summary>Maintains the final murder beam through the Baby's fatal impact sequence.</summary>
    FinalBabyMetroidAttackHolding,
    /// <summary>Creates distance from Samus and the Baby's death location before phase-three combat.</summary>
    Phase3RecoverFromCutsceneMakeSomeDistance,
    /// <summary>Restores phase-three combat posture, services, and attack selection.</summary>
    Phase3RecoverFromCutsceneSetupForFighting,
    /// <summary>Runs the ordinary phase-three movement and attack-choice loop.</summary>
    Phase3FightingMain,
    /// <summary>Waits out the phase-three delay before another combat action may be selected.</summary>
    Phase3FightingAttackCooldown,
    /// <summary>Moves the defeated body toward the rear of the arena to begin its death sequence.</summary>
    Phase3DeathSequenceMoveToBackOfRoom,
    /// <summary>Holds the body in place while timed death explosions are emitted.</summary>
    Phase3DeathSequenceIdleWhilstExploding,
    /// <summary>Staggers the damaged body back toward the arena center.</summary>
    Phase3DeathSequenceStumbleToMiddleOfRoom,
    /// <summary>Stops the active brain palette and beam effects before body removal.</summary>
    Phase3DeathSequenceDisableBrainEffects,
    /// <summary>Initializes the body fade-out palette and countdown.</summary>
    Phase3DeathSequenceSetupBodyFadeOut,
    /// <summary>Advances the body fade until the standing sprite is no longer visible.</summary>
    Phase3DeathSequenceFadeOutBody,
    /// <summary>Emits the final explosion batch after the body fade completes.</summary>
    Phase3DeathSequenceFinalFewExplosions,
    /// <summary>Pauses on the exposed brain before the decapitated fall begins.</summary>
    Phase3DeathSequenceRealizeDecapitation,
    /// <summary>Applies the decapitated brain's downward motion until it reaches the floor.</summary>
    Phase3DeathSequenceBrainFallsToGround,
    /// <summary>Transfers the corpse tile set needed for the grounded death artwork.</summary>
    Phase3DeathSequenceLoadCorpseTiles,
    /// <summary>Initializes the corpse palette transition to grey.</summary>
    Phase3DeathSequenceSetupFadeToGrey,
    /// <summary>Advances the grounded corpse palette until it is fully grey.</summary>
    Phase3DeathSequenceFadeToGrey,
    /// <summary>Animates the grey corpse tipping from upright to its fallen pose.</summary>
    Phase3DeathSequenceCorpseTipsOver,
    /// <summary>Runs corpse decay, dust emissions, and VRAM tile replacements.</summary>
    Phase3DeathSequenceCorpseRotsAway,
    /// <summary>Preserves the native twenty-update pause after the corpse disappears.</summary>
    Phase3DeathSequence20FrameDelay,
    /// <summary>Transfers the escape-timer graphics required by the impending countdown.</summary>
    Phase3DeathSequenceLoadEscapeTimerTiles,
    /// <summary>Starts escape music, timer state, boss completion, and room transition setup.</summary>
    Phase3DeathSequenceStartEscape,
    /// <summary>Requests the TIME BOMB SET subtitle object during the escape handoff.</summary>
    Phase3DeathSequenceSpawnTimeBombSetSubtitle,
    /// <summary>Advances the bank-$8B typewriter for the Zebes escape warning.</summary>
    Phase3DeathSequenceTypeOutZebesEscapeText,
    /// <summary>Starts the delay before the arena's escape door explodes.</summary>
    Phase3DeathSequenceDoorExplodingStartTimer,
    /// <summary>Emits the door explosion, fragments, palette change, and replacement PLM.</summary>
    Phase3DeathSequenceBlowUpEscapeDoor,
    /// <summary>Refreshes the earthquake timer while the escape sequence proceeds.</summary>
    Phase3DeathSequenceKeepEarthquakeGoing,
}

/// <summary>Reachable third-phase walking function pointers at `$A9:C26A-$C326`.</summary>
public enum MotherBrainPhase3WalkingPhase
{
    /// <summary>No phase-three horizontal walking function is active.</summary>
    Inactive,
    /// <summary>Attempts a small forward step while respecting Samus spacing and arena bounds.</summary>
    TryToInchForward,
    /// <summary>Moves rapidly away from Samus until the quick-retreat condition completes.</summary>
    RetreatQuickly,
    /// <summary>Moves away at the slower retreat speed used to re-establish attack spacing.</summary>
    RetreatSlowly,
}

/// <summary>Reachable third-phase neck function pointers at `$A9:C330-$C3EE`.</summary>
public enum MotherBrainPhase3NeckPhase
{
    /// <summary>No phase-three neck sub-function is active.</summary>
    Inactive,
    /// <summary>Tracks the ordinary phase-three head posture without recoil displacement.</summary>
    Normal,
    /// <summary>Initializes recovery from an ordinary weapon-hit neck recoil.</summary>
    SetupRecoilRecovery,
    /// <summary>Interpolates the recoiled neck back to its normal combat posture.</summary>
    RecoilRecovery,
    /// <summary>Initializes the stronger recoil caused by Samus's Hyper Beam.</summary>
    SetupHyperBeamRecoil,
    /// <summary>Runs the Hyper Beam recoil displacement and recovery timing.</summary>
    HyperBeamRecoil,
}

/// <summary>Low-three-bit projectile classes consumed by `$A9:B58E`.</summary>
public enum MotherBrainProjectileType
{
    /// <summary>Low-three-bit class zero: an ordinary beam projectile for damage lookup.</summary>
    Beam = 0,
    /// <summary>Low-three-bit class one: an ordinary missile projectile for damage lookup.</summary>
    Missile = 1,
    /// <summary>Low-three-bit class two: a super missile projectile for damage lookup.</summary>
    SuperMissile = 2,
    /// <summary>Low-three-bit class three: the Power Bomb family.</summary>
    PowerBomb = 3,
    /// <summary>Low-three-bit class four: the projectile family nibble $4, whose producer is not translated.</summary>
    ClassFour = 4,
    /// <summary>Low-three-bit class five: the bomb family.</summary>
    Bomb = 5,
    /// <summary>Low-three-bit class six: the projectile family nibble $6, whose producer is not translated.</summary>
    ClassSix = 6,
    /// <summary>Low-three-bit class seven: the beam-explosion family.</summary>
    BeamExplosion = 7,
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
    PlmHeaderId PlmEntry);

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
