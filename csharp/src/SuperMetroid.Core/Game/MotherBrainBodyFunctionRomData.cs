namespace SuperMetroid.Core.Game;

/// <summary>
/// Native function pointers used by Mother Brain's body dispatcher in bank <c>$A9</c>.
/// Keeping the cartridge addresses as enum values makes a debugger watch line up with the
/// disassembly without pretending that unrelated phases are interchangeable host states.
/// </summary>
public enum MotherBrainBodyFunction : ushort
{
    FirstPhase = 0x87e1,
    FakeDeathDescentInitialPause = 0x881d,
    FakeDeathDescentPauseBeforeLock = 0x8829,
    FakeDeathDescentPauseBeforeMusic = 0x884d,
    FakeDeathDescentPauseBeforeUnlock = 0x886c,
    FakeDeathDescentPauseBeforeFlash = 0x8884,
    FakeDeathDescentFadeToGray = 0x88b2,
    FakeDeathDescentCollapseTubes = 0x88d3,
    FakeDeathAscentDrawRows2And3 = 0x8c87,
    FakeDeathAscentDrawRows4And5 = 0x8c9e,
    FakeDeathAscentDrawRows6And7 = 0x8cb5,
    FakeDeathAscentDrawRows8And9 = 0x8ccc,
    FakeDeathAscentDrawRowsAAndB = 0x8ce3,
    FakeDeathAscentDrawRowsCAndD = 0x8cfa,
    FakeDeathAscentSetupPhase2Graphics = 0x8d11,
    FakeDeathAscentSetupPhase2Brain = 0x8d49,
    FakeDeathAscentPauseForSuspense = 0x8d79,
    FakeDeathAscentPrepareForRising = 0x8d8b,
    FakeDeathAscentLoadLegTiles = 0x8db4,
    FakeDeathAscentContinuePausing = 0x8dc3,
    FakeDeathAscentStartMusicAndEarthquake = 0x8dec,
    FakeDeathAscentRaiseMotherBrain = 0x8e4d,
    FakeDeathAscentWaitUntilUncrouched = 0x8e95,
    FakeDeathAscentTransitionFromGray = 0x8eaa,
    SecondPhaseStretchingShakeHead = 0x8ef5,
    SecondPhaseStretchingBringHeadUp = 0x8f14,
    SecondPhaseStretchingFinish = 0x8f33,
    SecondPhaseThinking = 0xb605,
    SecondPhaseTryAttack = 0xb64b,
    SecondPhaseBombDecideWalking = 0xb781,
    SecondPhaseBombWalkingBackwards = 0xb7ac,
    SecondPhaseBombCrouch = 0xb7c6,
    SecondPhaseBombFired = 0xb7e8,
    SecondPhaseBombStandUp = 0xb7f8,
    SecondPhaseLaserPositionHeadQuickly = 0xb80e,
    SecondPhaseLaserPositionHeadSlowlyAndFire = 0xb839,
    SecondPhaseLaserFinishAttack = 0xb863,
    SecondPhaseHandBeam = 0xb87d,
    SecondPhaseRainbowExtendNeck = 0xb8eb,
    SecondPhaseRainbowStartCharging = 0xb91a,
    SecondPhaseRainbowRetractNeck = 0xb92b,
    SecondPhaseRainbowWaitForCharge = 0xb93f,
    SecondPhaseRainbowExtendNeckDown = 0xb951,
    SecondPhaseRainbowStartFiring = 0xb975,
    SecondPhaseRainbowMoveSamusTowardWall = 0xb9e5,
    SecondPhaseRainbowOneFrameDelay = 0xba00,
    SecondPhaseRainbowStartDrainingSamus = 0xba27,
    SecondPhaseRainbowDrainingSamus = 0xba3c,
    SecondPhaseRainbowFinishFiring = 0xba5e,
    SecondPhaseRainbowLetSamusFall = 0xbac4,
    SecondPhaseRainbowWaitForSamusToLand = 0xbad1,
    SecondPhaseRainbowLowerHead = 0xbadd,
    SecondPhaseRainbowDecideNextAction = 0xbb06,
    SecondPhaseFinishSamusOff = 0xbd45,
    SecondPhaseFinishSamusOffStandUp = 0xbd98,
    SecondPhaseFinishSamusOffAdmire = 0xbda9,
    SecondPhaseFinishSamusOffChargeFinalBeam = 0xbdc1,
    SecondPhaseFinishSamusOffLoadBabyTiles = 0xbdd2,
    SecondPhaseFinishSamusOffFireFinalBeam = 0xbded,
    SecondPhaseFinalRainbowBeamHolding = 0xbe1a,
    SecondPhaseDrainedByBabyTakenAback = 0xbe38,
    SecondPhaseDrainedByBabyRegainBalance = 0xbe5d,
    SecondPhaseDrainedByBabyFiringRainbowBeam = 0xbe96,
    SecondPhaseDrainedByBabyRainbowBeamRunOut = 0xbf0e,
    SecondPhaseDrainedByBabyMoveToBackOfRoom = 0xbf41,
    SecondPhaseDrainedByBabyGoIntoLowPowerMode = 0xbf56,
    SecondPhaseDrainedByBabyPrepareTransitionToGrey = 0xbf7d,
    SecondPhaseDrainedByBabyTransitionToGrey = 0xbf95,
    SecondPhaseReviveInanimateGrey = 0xc059,
    SecondPhaseReviveShowSignsOfLife = 0xc066,
    SecondPhaseReviveTransitionFromGrey = 0xc08f,
    SecondPhaseReviveWakeUp = 0xc0ba,
    SecondPhaseReviveWakeUpStretch = 0xc0e4,
    SecondPhaseReviveWalkUpToBaby = 0xc0fb,
    SecondPhaseRevivePrepareNeckForBabyDeath = 0xc11e,
    SecondPhaseReviveFinishPreparingForBabyDeath = 0xc147,
    SecondPhaseMurderBabyAttack = 0xc15c,
    SecondPhaseMurderBabyAttackCooldown = 0xc182,
    SecondPhasePrepareForFinalBabyAttack = 0xc18e,
    SecondPhaseExecuteFinalBabyAttack = 0xc19a,
    SecondPhaseFinalBabyAttackHolding = 0xc1a6,
    ThirdPhaseRecoverMakeSomeDistance = 0xc1cf,
    ThirdPhaseRecoverSetupForFighting = 0xc1f0,
    ThirdPhaseFightingMain = 0xc209,
    ThirdPhaseFightingAttackCooldown = 0xc24e,
    /// <summary>$A9:AEE1, native phase-three death/escape: MoveToBackOfRoom.</summary>
    ThirdPhaseDeathMoveToBackOfRoom = 0xaee1,
    /// <summary>$A9:AF12, native phase-three death/escape: IdleWhilstExploding.</summary>
    ThirdPhaseDeathIdleWhilstExploding = 0xaf12,
    /// <summary>$A9:AF21, native phase-three death/escape: StumbleToMiddleOfRoom.</summary>
    ThirdPhaseDeathStumbleToMiddleOfRoom = 0xaf21,
    /// <summary>$A9:AF54, native phase-three death/escape: DisableBrainEffects.</summary>
    ThirdPhaseDeathDisableBrainEffects = 0xaf54,
    /// <summary>$A9:AF9D, native phase-three death/escape: SetupBodyFadeOut.</summary>
    ThirdPhaseDeathSetupBodyFadeOut = 0xaf9d,
    /// <summary>$A9:AFB6, native phase-three death/escape: FadeOutBody.</summary>
    ThirdPhaseDeathFadeOutBody = 0xafb6,
    /// <summary>$A9:B013, native phase-three death/escape: FinalFewExplosions.</summary>
    ThirdPhaseDeathFinalFewExplosions = 0xb013,
    /// <summary>$A9:B115, native phase-three death/escape: RealizeDecapitation.</summary>
    ThirdPhaseDeathRealizeDecapitation = 0xb115,
    /// <summary>$A9:B12D, native phase-three death/escape: BrainFallsToGround.</summary>
    ThirdPhaseDeathBrainFallsToGround = 0xb12d,
    /// <summary>$A9:B15E, native phase-three death/escape: LoadCorpseTiles.</summary>
    ThirdPhaseDeathLoadCorpseTiles = 0xb15e,
    /// <summary>$A9:B173, native phase-three death/escape: SetupFadeToGrey.</summary>
    ThirdPhaseDeathSetupFadeToGrey = 0xb173,
    /// <summary>$A9:B189, native phase-three death/escape: FadeToGrey.</summary>
    ThirdPhaseDeathFadeToGrey = 0xb189,
    /// <summary>$A9:B1B8, native phase-three death/escape: CorpseTipsOver.</summary>
    ThirdPhaseDeathCorpseTipsOver = 0xb1b8,
    /// <summary>$A9:B1D5, native phase-three death/escape: CorpseRotsAway.</summary>
    ThirdPhaseDeathCorpseRotsAway = 0xb1d5,
    /// <summary>$A9:B211, native phase-three death/escape: 20FrameDelay.</summary>
    ThirdPhaseDeath20FrameDelay = 0xb211,
    /// <summary>$A9:B258, native phase-three death/escape: LoadEscapeTimerTiles.</summary>
    ThirdPhaseDeathLoadEscapeTimerTiles = 0xb258,
    /// <summary>$A9:B26D, native phase-three death/escape: StartEscape.</summary>
    ThirdPhaseDeathStartEscape = 0xb26d,
    /// <summary>$A9:B2D1, native phase-three death/escape: SpawnTimeBombSetSubtitle.</summary>
    ThirdPhaseDeathSpawnTimeBombSetSubtitle = 0xb2d1,
    /// <summary>$A9:B2E3, native phase-three death/escape: TypeOutZebesEscapeText.</summary>
    ThirdPhaseDeathTypeOutZebesEscapeText = 0xb2e3,
    /// <summary>$A9:B2F9, native phase-three death/escape: DoorExplodingStartTimer.</summary>
    ThirdPhaseDeathDoorExplodingStartTimer = 0xb2f9,
    /// <summary>$A9:B32A, native phase-three death/escape: BlowUpEscapeDoor.</summary>
    ThirdPhaseDeathBlowUpEscapeDoor = 0xb32a,
    /// <summary>$A9:B33C, native phase-three death/escape: KeepEarthquakeGoing.</summary>
    ThirdPhaseDeathKeepEarthquakeGoing = 0xb33c,
}
