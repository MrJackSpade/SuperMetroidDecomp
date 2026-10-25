using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Verification's native view of the Mother Brain encounter's identities (#1413): the cartridge routine words naming
/// each member. Core's values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class MotherBrainNativeWords
{
    /// <summary>The bank-$A9 routine word naming each <see cref="MotherBrainBodyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<MotherBrainBodyFunction> MotherBrainBodyFunctions = new(new Dictionary<MotherBrainBodyFunction, ushort>
    {
        [MotherBrainBodyFunction.FirstPhase] = 0x87e1,
        [MotherBrainBodyFunction.FakeDeathDescentInitialPause] = 0x881d,
        [MotherBrainBodyFunction.FakeDeathDescentPauseBeforeLock] = 0x8829,
        [MotherBrainBodyFunction.FakeDeathDescentPauseBeforeMusic] = 0x884d,
        [MotherBrainBodyFunction.FakeDeathDescentPauseBeforeUnlock] = 0x886c,
        [MotherBrainBodyFunction.FakeDeathDescentPauseBeforeFlash] = 0x8884,
        [MotherBrainBodyFunction.FakeDeathDescentFadeToGray] = 0x88b2,
        [MotherBrainBodyFunction.FakeDeathDescentCollapseTubes] = 0x88d3,
        [MotherBrainBodyFunction.FakeDeathAscentDrawRows2And3] = 0x8c87,
        [MotherBrainBodyFunction.FakeDeathAscentDrawRows4And5] = 0x8c9e,
        [MotherBrainBodyFunction.FakeDeathAscentDrawRows6And7] = 0x8cb5,
        [MotherBrainBodyFunction.FakeDeathAscentDrawRows8And9] = 0x8ccc,
        [MotherBrainBodyFunction.FakeDeathAscentDrawRowsAAndB] = 0x8ce3,
        [MotherBrainBodyFunction.FakeDeathAscentDrawRowsCAndD] = 0x8cfa,
        [MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Graphics] = 0x8d11,
        [MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Brain] = 0x8d49,
        [MotherBrainBodyFunction.FakeDeathAscentPauseForSuspense] = 0x8d79,
        [MotherBrainBodyFunction.FakeDeathAscentPrepareForRising] = 0x8d8b,
        [MotherBrainBodyFunction.FakeDeathAscentLoadLegTiles] = 0x8db4,
        [MotherBrainBodyFunction.FakeDeathAscentContinuePausing] = 0x8dc3,
        [MotherBrainBodyFunction.FakeDeathAscentStartMusicAndEarthquake] = 0x8dec,
        [MotherBrainBodyFunction.FakeDeathAscentRaiseMotherBrain] = 0x8e4d,
        [MotherBrainBodyFunction.FakeDeathAscentWaitUntilUncrouched] = 0x8e95,
        [MotherBrainBodyFunction.FakeDeathAscentTransitionFromGray] = 0x8eaa,
        [MotherBrainBodyFunction.SecondPhaseStretchingShakeHead] = 0x8ef5,
        [MotherBrainBodyFunction.SecondPhaseStretchingBringHeadUp] = 0x8f14,
        [MotherBrainBodyFunction.SecondPhaseStretchingFinish] = 0x8f33,
        [MotherBrainBodyFunction.SecondPhaseThinking] = 0xb605,
        [MotherBrainBodyFunction.SecondPhaseTryAttack] = 0xb64b,
        [MotherBrainBodyFunction.SecondPhaseBombDecideWalking] = 0xb781,
        [MotherBrainBodyFunction.SecondPhaseBombWalkingBackwards] = 0xb7ac,
        [MotherBrainBodyFunction.SecondPhaseBombCrouch] = 0xb7c6,
        [MotherBrainBodyFunction.SecondPhaseBombFired] = 0xb7e8,
        [MotherBrainBodyFunction.SecondPhaseBombStandUp] = 0xb7f8,
        [MotherBrainBodyFunction.SecondPhaseLaserPositionHeadQuickly] = 0xb80e,
        [MotherBrainBodyFunction.SecondPhaseLaserPositionHeadSlowlyAndFire] = 0xb839,
        [MotherBrainBodyFunction.SecondPhaseLaserFinishAttack] = 0xb863,
        [MotherBrainBodyFunction.SecondPhaseHandBeam] = 0xb87d,
        [MotherBrainBodyFunction.SecondPhaseRainbowExtendNeck] = 0xb8eb,
        [MotherBrainBodyFunction.SecondPhaseRainbowStartCharging] = 0xb91a,
        [MotherBrainBodyFunction.SecondPhaseRainbowRetractNeck] = 0xb92b,
        [MotherBrainBodyFunction.SecondPhaseRainbowWaitForCharge] = 0xb93f,
        [MotherBrainBodyFunction.SecondPhaseRainbowExtendNeckDown] = 0xb951,
        [MotherBrainBodyFunction.SecondPhaseRainbowStartFiring] = 0xb975,
        [MotherBrainBodyFunction.SecondPhaseRainbowMoveSamusTowardWall] = 0xb9e5,
        [MotherBrainBodyFunction.SecondPhaseRainbowOneFrameDelay] = 0xba00,
        [MotherBrainBodyFunction.SecondPhaseRainbowStartDrainingSamus] = 0xba27,
        [MotherBrainBodyFunction.SecondPhaseRainbowDrainingSamus] = 0xba3c,
        [MotherBrainBodyFunction.SecondPhaseRainbowFinishFiring] = 0xba5e,
        [MotherBrainBodyFunction.SecondPhaseRainbowLetSamusFall] = 0xbac4,
        [MotherBrainBodyFunction.SecondPhaseRainbowWaitForSamusToLand] = 0xbad1,
        [MotherBrainBodyFunction.SecondPhaseRainbowLowerHead] = 0xbadd,
        [MotherBrainBodyFunction.SecondPhaseRainbowDecideNextAction] = 0xbb06,
        [MotherBrainBodyFunction.SecondPhaseFinishSamusOff] = 0xbd45,
        [MotherBrainBodyFunction.SecondPhaseFinishSamusOffStandUp] = 0xbd98,
        [MotherBrainBodyFunction.SecondPhaseFinishSamusOffAdmire] = 0xbda9,
        [MotherBrainBodyFunction.SecondPhaseFinishSamusOffChargeFinalBeam] = 0xbdc1,
        [MotherBrainBodyFunction.SecondPhaseFinishSamusOffLoadBabyTiles] = 0xbdd2,
        [MotherBrainBodyFunction.SecondPhaseFinishSamusOffFireFinalBeam] = 0xbded,
        [MotherBrainBodyFunction.SecondPhaseFinalRainbowBeamHolding] = 0xbe1a,
        [MotherBrainBodyFunction.SecondPhaseDrainedByBabyTakenAback] = 0xbe38,
        [MotherBrainBodyFunction.SecondPhaseDrainedByBabyRegainBalance] = 0xbe5d,
        [MotherBrainBodyFunction.SecondPhaseDrainedByBabyFiringRainbowBeam] = 0xbe96,
        [MotherBrainBodyFunction.SecondPhaseDrainedByBabyRainbowBeamRunOut] = 0xbf0e,
        [MotherBrainBodyFunction.SecondPhaseDrainedByBabyMoveToBackOfRoom] = 0xbf41,
        [MotherBrainBodyFunction.SecondPhaseDrainedByBabyGoIntoLowPowerMode] = 0xbf56,
        [MotherBrainBodyFunction.SecondPhaseDrainedByBabyPrepareTransitionToGrey] = 0xbf7d,
        [MotherBrainBodyFunction.SecondPhaseDrainedByBabyTransitionToGrey] = 0xbf95,
        [MotherBrainBodyFunction.SecondPhaseReviveInanimateGrey] = 0xc059,
        [MotherBrainBodyFunction.SecondPhaseReviveShowSignsOfLife] = 0xc066,
        [MotherBrainBodyFunction.SecondPhaseReviveTransitionFromGrey] = 0xc08f,
        [MotherBrainBodyFunction.SecondPhaseReviveWakeUp] = 0xc0ba,
        [MotherBrainBodyFunction.SecondPhaseReviveWakeUpStretch] = 0xc0e4,
        [MotherBrainBodyFunction.SecondPhaseReviveWalkUpToBaby] = 0xc0fb,
        [MotherBrainBodyFunction.SecondPhaseRevivePrepareNeckForBabyDeath] = 0xc11e,
        [MotherBrainBodyFunction.SecondPhaseReviveFinishPreparingForBabyDeath] = 0xc147,
        [MotherBrainBodyFunction.SecondPhaseMurderBabyAttack] = 0xc15c,
        [MotherBrainBodyFunction.SecondPhaseMurderBabyAttackCooldown] = 0xc182,
        [MotherBrainBodyFunction.SecondPhasePrepareForFinalBabyAttack] = 0xc18e,
        [MotherBrainBodyFunction.SecondPhaseExecuteFinalBabyAttack] = 0xc19a,
        [MotherBrainBodyFunction.SecondPhaseFinalBabyAttackHolding] = 0xc1a6,
        [MotherBrainBodyFunction.ThirdPhaseRecoverMakeSomeDistance] = 0xc1cf,
        [MotherBrainBodyFunction.ThirdPhaseRecoverSetupForFighting] = 0xc1f0,
        [MotherBrainBodyFunction.ThirdPhaseFightingMain] = 0xc209,
        [MotherBrainBodyFunction.ThirdPhaseFightingAttackCooldown] = 0xc24e,
        [MotherBrainBodyFunction.ThirdPhaseDeathMoveToBackOfRoom] = 0xaee1,
        [MotherBrainBodyFunction.ThirdPhaseDeathIdleWhilstExploding] = 0xaf12,
        [MotherBrainBodyFunction.ThirdPhaseDeathStumbleToMiddleOfRoom] = 0xaf21,
        [MotherBrainBodyFunction.ThirdPhaseDeathDisableBrainEffects] = 0xaf54,
        [MotherBrainBodyFunction.ThirdPhaseDeathSetupBodyFadeOut] = 0xaf9d,
        [MotherBrainBodyFunction.ThirdPhaseDeathFadeOutBody] = 0xafb6,
        [MotherBrainBodyFunction.ThirdPhaseDeathFinalFewExplosions] = 0xb013,
        [MotherBrainBodyFunction.ThirdPhaseDeathRealizeDecapitation] = 0xb115,
        [MotherBrainBodyFunction.ThirdPhaseDeathBrainFallsToGround] = 0xb12d,
        [MotherBrainBodyFunction.ThirdPhaseDeathLoadCorpseTiles] = 0xb15e,
        [MotherBrainBodyFunction.ThirdPhaseDeathSetupFadeToGrey] = 0xb173,
        [MotherBrainBodyFunction.ThirdPhaseDeathFadeToGrey] = 0xb189,
        [MotherBrainBodyFunction.ThirdPhaseDeathCorpseTipsOver] = 0xb1b8,
        [MotherBrainBodyFunction.ThirdPhaseDeathCorpseRotsAway] = 0xb1d5,
        [MotherBrainBodyFunction.ThirdPhaseDeath20FrameDelay] = 0xb211,
        [MotherBrainBodyFunction.ThirdPhaseDeathLoadEscapeTimerTiles] = 0xb258,
        [MotherBrainBodyFunction.ThirdPhaseDeathStartEscape] = 0xb26d,
        [MotherBrainBodyFunction.ThirdPhaseDeathSpawnTimeBombSetSubtitle] = 0xb2d1,
        [MotherBrainBodyFunction.ThirdPhaseDeathTypeOutZebesEscapeText] = 0xb2e3,
        [MotherBrainBodyFunction.ThirdPhaseDeathDoorExplodingStartTimer] = 0xb2f9,
        [MotherBrainBodyFunction.ThirdPhaseDeathBlowUpEscapeDoor] = 0xb32a,
        [MotherBrainBodyFunction.ThirdPhaseDeathKeepEarthquakeGoing] = 0xb33c,
    });

    /// <summary>The bank-$A2 routine word naming each <see cref="RinkaEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<RinkaEnemyFunction> RinkaEnemyFunctions = new(new Dictionary<RinkaEnemyFunction, ushort>
    {
        [RinkaEnemyFunction.AimDelay] = 0xb7df,
        [RinkaEnemyFunction.DeathRespawnDelay] = 0xb844,
        [RinkaEnemyFunction.WatchForLeavingViewport] = 0xb852,
        [RinkaEnemyFunction.Flying] = 0xb85b,
    });

    /// <summary>The bank-$A9 routine word naming each <see cref="BabyMetroidRouteMovementFunction"/>.</summary>
    internal static readonly NativeWordIdentities<BabyMetroidRouteMovementFunction> BabyMetroidRouteMovementFunctions = new(new Dictionary<BabyMetroidRouteMovementFunction, ushort>
    {
        [BabyMetroidRouteMovementFunction.GradualAccelerationExtraEight] = 0xf45f,
        [BabyMetroidRouteMovementFunction.GradualAccelerationExtraSixteen] = 0xf466,
    });

    /// <summary>The bank-$A9 routine word naming each <see cref="ShitroidAiFunction"/>.</summary>
    internal static readonly NativeWordIdentities<ShitroidAiFunction> ShitroidAiFunctions = new(new Dictionary<ShitroidAiFunction, ushort>
    {
        [ShitroidAiFunction.Dormant] = 0xefdf,
        [ShitroidAiFunction.WaitForCamera] = 0xefe6,
        [ShitroidAiFunction.BeginEntranceDelay] = 0xf02b,
        [ShitroidAiFunction.EntranceDelay] = 0xf037,
        [ShitroidAiFunction.FlyToSidehopper] = 0xf049,
        [ShitroidAiFunction.ChaseSidehopper] = 0xf06d,
        [ShitroidAiFunction.AttachToSidehopper] = 0xf094,
        [ShitroidAiFunction.DrainSidehopper] = 0xf0e6,
        [ShitroidAiFunction.ActivateSidehopperCorpse] = 0xf125,
        [ShitroidAiFunction.RiseAfterFeeding] = 0xf138,
        [ShitroidAiFunction.HoverNearSamus] = 0xf180,
        [ShitroidAiFunction.ChaseSamus] = 0xf1fa,
        [ShitroidAiFunction.BeginDrainingSamus] = 0xf20e,
        [ShitroidAiFunction.DrainSamus] = 0xf21b,
        [ShitroidAiFunction.BeginPostDrainPause] = 0xf2a2,
        [ShitroidAiFunction.PostDrainPause] = 0xf2ae,
        [ShitroidAiFunction.RiseAfterDrainingSamus] = 0xf2c0,
        [ShitroidAiFunction.FlyLeft] = 0xf2fb,
        [ShitroidAiFunction.FlyRight] = 0xf324,
        [ShitroidAiFunction.BeginExit] = 0xf360,
        [ShitroidAiFunction.Exit] = 0xf36d,
        [ShitroidAiFunction.HoldSamusBeforeRelease] = 0xf3a3,
        [ShitroidAiFunction.ReleasedFollow] = 0xf3be,
    });

    /// <summary>The glass-shard spawn parameter (the emitter's byte offset into the placement tables) naming each <see cref="MotherBrainGlassShard"/>.</summary>
    internal static readonly NativeWordIdentities<MotherBrainGlassShard> GlassShards = new(new Dictionary<MotherBrainGlassShard, ushort>
    {
        [MotherBrainGlassShard.Right] = 0,
        [MotherBrainGlassShard.Left] = 2,
        [MotherBrainGlassShard.Centre] = 4,
    });

    /// <summary>The bank-$A9 routine word naming each <see cref="MotherBrainTubeFunction"/>.</summary>
    internal static readonly NativeWordIdentities<MotherBrainTubeFunction> MotherBrainTubeFunctions = new(new Dictionary<MotherBrainTubeFunction, ushort>
    {
        [MotherBrainTubeFunction.NonMainTube] = 0x8b88,
        [MotherBrainTubeFunction.MainTube_WaitingToFall] = 0x8bcb,
        [MotherBrainTubeFunction.MainTube_Falling] = 0x8bd6,
    });

    /// <summary>The bank-$A6 routine word naming each <see cref="ZebetiteAiFunction"/>.</summary>
    internal static readonly NativeWordIdentities<ZebetiteAiFunction> ZebetiteAiFunctions = new(new Dictionary<ZebetiteAiFunction, ushort>
    {
        [ZebetiteAiFunction.SpawnLinkedHalf] = 0xfc41,
        [ZebetiteAiFunction.WaitForDoorTransition] = 0xfc5b,
        [ZebetiteAiFunction.Active] = 0xfc67,
    });

    /// <summary>The cartridge's sign bit of a Zebetite generation's variable F: set for a split pair.</summary>
    private const ushort LinkedPairWord = 0x8000;

    /// <summary>Decodes a Zebetite generation word, which the cartridge sets only to zero or its sign bit.</summary>
    internal static ZebetiteGenerationFlags DecodeGenerationFlags(ushort word) => word switch
    {
        0 => ZebetiteGenerationFlags.None,
        LinkedPairWord => ZebetiteGenerationFlags.LinkedPair,
        _ => throw new InvalidDataException($"${word:X4} is not a Zebetite generation word."),
    };

    /// <summary>Encodes a Zebetite generation's flags as the cartridge's variable F word.</summary>
    internal static ushort GenerationFlagsWord(ZebetiteGenerationFlags flags) => flags switch
    {
        ZebetiteGenerationFlags.None => 0,
        ZebetiteGenerationFlags.LinkedPair => LinkedPairWord,
        _ => throw new ArgumentOutOfRangeException(nameof(flags), flags, "Undefined Zebetite generation flags."),
    };
}
