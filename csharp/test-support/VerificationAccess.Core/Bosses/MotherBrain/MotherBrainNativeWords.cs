using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Verification's native view of the Mother Brain encounter's identities (#1413): the cartridge routine words naming
/// each member. Core's values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class MotherBrainNativeWords
{
    /// <summary>
    /// The byte offset into the neck motion dispatchers' five-entry jump tables naming each <see cref="MotherBrainNeckMovement"/>;
    /// both joints' tables share the layout.
    /// </summary>
    internal static readonly NativeWordIdentities<MotherBrainNeckMovement> NeckMovements = new(new Dictionary<MotherBrainNeckMovement, ushort>
    {
        [MotherBrainNeckMovement.None] = 0,
        [MotherBrainNeckMovement.BobDown] = 2,
        [MotherBrainNeckMovement.BobUp] = 4,
        [MotherBrainNeckMovement.Lower] = 6,
        [MotherBrainNeckMovement.Raise] = 8,
    });

    /// <summary>The attack dispatcher's index naming each <see cref="MotherBrainAttackPhase"/>.</summary>
    internal static readonly NativeWordIdentities<MotherBrainAttackPhase> AttackPhases = new(new Dictionary<MotherBrainAttackPhase, ushort>
    {
        [MotherBrainAttackPhase.ChooseAttack] = 0,
        [MotherBrainAttackPhase.Cooldown] = 1,
        [MotherBrainAttackPhase.EndAttack] = 2,
    });

    /// <summary>The hand-beam dispatcher's index naming each <see cref="MotherBrainHandBeamPhase"/>.</summary>
    internal static readonly NativeWordIdentities<MotherBrainHandBeamPhase> HandBeamPhases = new(new Dictionary<MotherBrainHandBeamPhase, ushort>
    {
        [MotherBrainHandBeamPhase.BackUp] = 0,
        [MotherBrainHandBeamPhase.WaitForBombs] = 1,
        [MotherBrainHandBeamPhase.Firing] = 2,
        [MotherBrainHandBeamPhase.Finish] = 3,
    });

    /// <summary>The bank-$A9 routine word naming each <see cref="MotherBrainBodyPhase"/>.</summary>
    internal static readonly NativeWordIdentities<MotherBrainBodyPhase> MotherBrainBodyPhases = new(new Dictionary<MotherBrainBodyPhase, ushort>
    {
        [MotherBrainBodyPhase.FirstPhase] = 0x87e1,
        [MotherBrainBodyPhase.FakeDeathDescentInitialPause] = 0x881d,
        [MotherBrainBodyPhase.FakeDeathDescentPauseBeforeLock] = 0x8829,
        [MotherBrainBodyPhase.FakeDeathDescentPauseBeforeMusic] = 0x884d,
        [MotherBrainBodyPhase.FakeDeathDescentPauseBeforeUnlock] = 0x886c,
        [MotherBrainBodyPhase.FakeDeathDescentPauseBeforeFlash] = 0x8884,
        [MotherBrainBodyPhase.FakeDeathDescentFadeToGray] = 0x88b2,
        [MotherBrainBodyPhase.FakeDeathDescentCollapseTubes] = 0x88d3,
        [MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows1And2] = 0x8c87,
        [MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows3And4] = 0x8c9e,
        [MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows5And6] = 0x8cb5,
        [MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows7And8] = 0x8ccc,
        [MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows9And10] = 0x8ce3,
        [MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows11And12] = 0x8cfa,
        [MotherBrainBodyPhase.FakeDeathAscentSetupPhase2Graphics] = 0x8d11,
        [MotherBrainBodyPhase.FakeDeathAscentSetupPhase2Brain] = 0x8d49,
        [MotherBrainBodyPhase.FakeDeathAscentPauseForSuspense] = 0x8d79,
        [MotherBrainBodyPhase.FakeDeathAscentPrepareForRising] = 0x8d8b,
        [MotherBrainBodyPhase.FakeDeathAscentReadyStandingBody] = 0x8db4,
        [MotherBrainBodyPhase.FakeDeathAscentContinuePausing] = 0x8dc3,
        [MotherBrainBodyPhase.FakeDeathAscentStartMusicAndEarthquake] = 0x8dec,
        [MotherBrainBodyPhase.FakeDeathAscentRaiseMotherBrain] = 0x8e4d,
        [MotherBrainBodyPhase.FakeDeathAscentWaitUntilUncrouched] = 0x8e95,
        [MotherBrainBodyPhase.FakeDeathAscentTransitionFromGray] = 0x8eaa,
        [MotherBrainBodyPhase.SecondPhaseStretchingShakeHead] = 0x8ef5,
        [MotherBrainBodyPhase.SecondPhaseStretchingBringHeadUp] = 0x8f14,
        [MotherBrainBodyPhase.SecondPhaseStretchingFinish] = 0x8f33,
        [MotherBrainBodyPhase.SecondPhaseThinking] = 0xb605,
        [MotherBrainBodyPhase.SecondPhaseTryAttack] = 0xb64b,
        [MotherBrainBodyPhase.SecondPhaseBombDecideWalking] = 0xb781,
        [MotherBrainBodyPhase.SecondPhaseBombWalkingBackwards] = 0xb7ac,
        [MotherBrainBodyPhase.SecondPhaseBombCrouch] = 0xb7c6,
        [MotherBrainBodyPhase.SecondPhaseBombFired] = 0xb7e8,
        [MotherBrainBodyPhase.SecondPhaseBombStandUp] = 0xb7f8,
        [MotherBrainBodyPhase.SecondPhaseLaserPositionHeadQuickly] = 0xb80e,
        [MotherBrainBodyPhase.SecondPhaseLaserPositionHeadSlowlyAndFire] = 0xb839,
        [MotherBrainBodyPhase.SecondPhaseLaserFinishAttack] = 0xb863,
        [MotherBrainBodyPhase.SecondPhaseHandBeam] = 0xb87d,
        [MotherBrainBodyPhase.SecondPhaseRainbowExtendNeck] = 0xb8eb,
        [MotherBrainBodyPhase.SecondPhaseRainbowStartCharging] = 0xb91a,
        [MotherBrainBodyPhase.SecondPhaseRainbowRetractNeck] = 0xb92b,
        [MotherBrainBodyPhase.SecondPhaseRainbowWaitForCharge] = 0xb93f,
        [MotherBrainBodyPhase.SecondPhaseRainbowExtendNeckDown] = 0xb951,
        [MotherBrainBodyPhase.SecondPhaseRainbowStartFiring] = 0xb975,
        [MotherBrainBodyPhase.SecondPhaseRainbowMoveSamusTowardWall] = 0xb9e5,
        [MotherBrainBodyPhase.SecondPhaseRainbowOneFrameDelay] = 0xba00,
        [MotherBrainBodyPhase.SecondPhaseRainbowStartDrainingSamus] = 0xba27,
        [MotherBrainBodyPhase.SecondPhaseRainbowDrainingSamus] = 0xba3c,
        [MotherBrainBodyPhase.SecondPhaseRainbowFinishFiring] = 0xba5e,
        [MotherBrainBodyPhase.SecondPhaseRainbowLetSamusFall] = 0xbac4,
        [MotherBrainBodyPhase.SecondPhaseRainbowWaitForSamusToLand] = 0xbad1,
        [MotherBrainBodyPhase.SecondPhaseRainbowLowerHead] = 0xbadd,
        [MotherBrainBodyPhase.SecondPhaseRainbowDecideNextAction] = 0xbb06,
        [MotherBrainBodyPhase.SecondPhaseFinishSamusOff] = 0xbd45,
        [MotherBrainBodyPhase.SecondPhaseFinishSamusOffStandUp] = 0xbd98,
        [MotherBrainBodyPhase.SecondPhaseFinishSamusOffAdmire] = 0xbda9,
        [MotherBrainBodyPhase.SecondPhaseFinishSamusOffChargeFinalBeam] = 0xbdc1,
        [MotherBrainBodyPhase.SecondPhaseFinishSamusOffReadyBaby] = 0xbdd2,
        [MotherBrainBodyPhase.SecondPhaseFinishSamusOffFireFinalBeam] = 0xbded,
        [MotherBrainBodyPhase.SecondPhaseFinalRainbowBeamHolding] = 0xbe1a,
        [MotherBrainBodyPhase.SecondPhaseDrainedByBabyTakenAback] = 0xbe38,
        [MotherBrainBodyPhase.SecondPhaseDrainedByBabyRegainBalance] = 0xbe5d,
        [MotherBrainBodyPhase.SecondPhaseDrainedByBabyFiringRainbowBeam] = 0xbe96,
        [MotherBrainBodyPhase.SecondPhaseDrainedByBabyRainbowBeamRunOut] = 0xbf0e,
        [MotherBrainBodyPhase.SecondPhaseDrainedByBabyMoveToBackOfRoom] = 0xbf41,
        [MotherBrainBodyPhase.SecondPhaseDrainedByBabyGoIntoLowPowerMode] = 0xbf56,
        [MotherBrainBodyPhase.SecondPhaseDrainedByBabyPrepareTransitionToGrey] = 0xbf7d,
        [MotherBrainBodyPhase.SecondPhaseDrainedByBabyTransitionToGrey] = 0xbf95,
        [MotherBrainBodyPhase.SecondPhaseReviveInanimateGrey] = 0xc059,
        [MotherBrainBodyPhase.SecondPhaseReviveShowSignsOfLife] = 0xc066,
        [MotherBrainBodyPhase.SecondPhaseReviveTransitionFromGrey] = 0xc08f,
        [MotherBrainBodyPhase.SecondPhaseReviveWakeUp] = 0xc0ba,
        [MotherBrainBodyPhase.SecondPhaseReviveWakeUpStretch] = 0xc0e4,
        [MotherBrainBodyPhase.SecondPhaseReviveWalkUpToBaby] = 0xc0fb,
        [MotherBrainBodyPhase.SecondPhaseRevivePrepareNeckForBabyDeath] = 0xc11e,
        [MotherBrainBodyPhase.SecondPhaseReviveFinishPreparingForBabyDeath] = 0xc147,
        [MotherBrainBodyPhase.SecondPhaseMurderBabyAttack] = 0xc15c,
        [MotherBrainBodyPhase.SecondPhaseMurderBabyAttackCooldown] = 0xc182,
        [MotherBrainBodyPhase.SecondPhasePrepareForFinalBabyAttack] = 0xc18e,
        [MotherBrainBodyPhase.SecondPhaseExecuteFinalBabyAttack] = 0xc19a,
        [MotherBrainBodyPhase.SecondPhaseFinalBabyAttackHolding] = 0xc1a6,
        [MotherBrainBodyPhase.ThirdPhaseRecoverMakeSomeDistance] = 0xc1cf,
        [MotherBrainBodyPhase.ThirdPhaseRecoverSetupForFighting] = 0xc1f0,
        [MotherBrainBodyPhase.ThirdPhaseFightingMain] = 0xc209,
        [MotherBrainBodyPhase.ThirdPhaseFightingAttackCooldown] = 0xc24e,
        [MotherBrainBodyPhase.ThirdPhaseDeathMoveToBackOfRoom] = 0xaee1,
        [MotherBrainBodyPhase.ThirdPhaseDeathIdleWhilstExploding] = 0xaf12,
        [MotherBrainBodyPhase.ThirdPhaseDeathStumbleToMiddleOfRoom] = 0xaf21,
        [MotherBrainBodyPhase.ThirdPhaseDeathDisableBrainEffects] = 0xaf54,
        [MotherBrainBodyPhase.ThirdPhaseDeathSetupBodyFadeOut] = 0xaf9d,
        [MotherBrainBodyPhase.ThirdPhaseDeathFadeOutBody] = 0xafb6,
        [MotherBrainBodyPhase.ThirdPhaseDeathFinalFewExplosions] = 0xb013,
        [MotherBrainBodyPhase.ThirdPhaseDeathRealizeDecapitation] = 0xb115,
        [MotherBrainBodyPhase.ThirdPhaseDeathBrainFallsToGround] = 0xb12d,
        [MotherBrainBodyPhase.ThirdPhaseDeathReadyCorpse] = 0xb15e,
        [MotherBrainBodyPhase.ThirdPhaseDeathSetupFadeToGrey] = 0xb173,
        [MotherBrainBodyPhase.ThirdPhaseDeathFadeToGrey] = 0xb189,
        [MotherBrainBodyPhase.ThirdPhaseDeathCorpseTipsOver] = 0xb1b8,
        [MotherBrainBodyPhase.ThirdPhaseDeathCorpseRotsAway] = 0xb1d5,
        [MotherBrainBodyPhase.ThirdPhaseDeath20FrameDelay] = 0xb211,
        [MotherBrainBodyPhase.ThirdPhaseDeathReadyEscapeWarning] = 0xb258,
        [MotherBrainBodyPhase.ThirdPhaseDeathStartEscape] = 0xb26d,
        [MotherBrainBodyPhase.ThirdPhaseDeathSpawnTimeBombSetSubtitle] = 0xb2d1,
        [MotherBrainBodyPhase.ThirdPhaseDeathTypeOutZebesEscapeText] = 0xb2e3,
        [MotherBrainBodyPhase.ThirdPhaseDeathDoorExplodingStartTimer] = 0xb2f9,
        [MotherBrainBodyPhase.ThirdPhaseDeathBlowUpEscapeDoor] = 0xb32a,
        [MotherBrainBodyPhase.ThirdPhaseDeathKeepEarthquakeGoing] = 0xb33c,
    });

    /// <summary>The bank-$A2 routine word naming each <see cref="RinkaAction"/>.</summary>
    internal static readonly NativeWordIdentities<RinkaAction> RinkaActions = new(new Dictionary<RinkaAction, ushort>
    {
        [RinkaAction.AimDelay] = 0xb7df,
        [RinkaAction.DeathRespawnDelay] = 0xb844,
        [RinkaAction.WatchForLeavingViewport] = 0xb852,
        [RinkaAction.Flying] = 0xb85b,
    });

    /// <summary>
    /// The reservation token naming each <see cref="RinkaSpawnPoint"/>: twice the point's one-based place in the bank-$A2
    /// spawn-point table, which a special Rinka's record holds while it holds the point (zero names none).
    /// </summary>
    internal static readonly NativeWordIdentities<RinkaSpawnPoint> RinkaSpawnPoints = new(new Dictionary<RinkaSpawnPoint, ushort>
    {
        [RinkaSpawnPoint.FarRightUpper] = 2,
        [RinkaSpawnPoint.FarRightLower] = 4,
        [RinkaSpawnPoint.RightUpper] = 6,
        [RinkaSpawnPoint.RightLower] = 8,
        [RinkaSpawnPoint.CentreUpper] = 10,
        [RinkaSpawnPoint.CentreLower] = 12,
        [RinkaSpawnPoint.LeftUpper] = 14,
        [RinkaSpawnPoint.LeftLower] = 16,
        [RinkaSpawnPoint.FarLeftUpper] = 18,
        [RinkaSpawnPoint.FarLeftLower] = 20,
        [RinkaSpawnPoint.Leftmost] = 22,
    });

    /// <summary>The bank-$A9 routine word naming each <see cref="BabyMetroidRouteMovement"/>.</summary>
    internal static readonly NativeWordIdentities<BabyMetroidRouteMovement> BabyMetroidRouteMovements = new(new Dictionary<BabyMetroidRouteMovement, ushort>
    {
        [BabyMetroidRouteMovement.GradualAccelerationExtraEight] = 0xf45f,
        [BabyMetroidRouteMovement.GradualAccelerationExtraSixteen] = 0xf466,
    });

    /// <summary>The bank-$A9 routine word naming each <see cref="ShitroidAction"/>.</summary>
    internal static readonly NativeWordIdentities<ShitroidAction> ShitroidActions = new(new Dictionary<ShitroidAction, ushort>
    {
        [ShitroidAction.Dormant] = 0xefdf,
        [ShitroidAction.WaitForCamera] = 0xefe6,
        [ShitroidAction.BeginEntranceDelay] = 0xf02b,
        [ShitroidAction.EntranceDelay] = 0xf037,
        [ShitroidAction.FlyToSidehopper] = 0xf049,
        [ShitroidAction.ChaseSidehopper] = 0xf06d,
        [ShitroidAction.AttachToSidehopper] = 0xf094,
        [ShitroidAction.DrainSidehopper] = 0xf0e6,
        [ShitroidAction.ActivateSidehopperCorpse] = 0xf125,
        [ShitroidAction.RiseAfterFeeding] = 0xf138,
        [ShitroidAction.HoverNearSamus] = 0xf180,
        [ShitroidAction.ChaseSamus] = 0xf1fa,
        [ShitroidAction.BeginDrainingSamus] = 0xf20e,
        [ShitroidAction.DrainSamus] = 0xf21b,
        [ShitroidAction.BeginPostDrainPause] = 0xf2a2,
        [ShitroidAction.PostDrainPause] = 0xf2ae,
        [ShitroidAction.RiseAfterDrainingSamus] = 0xf2c0,
        [ShitroidAction.FlyLeft] = 0xf2fb,
        [ShitroidAction.FlyRight] = 0xf324,
        [ShitroidAction.BeginExit] = 0xf360,
        [ShitroidAction.Exit] = 0xf36d,
        [ShitroidAction.HoldSamusBeforeRelease] = 0xf3a3,
        [ShitroidAction.ReleasedFollow] = 0xf3be,
    });

    /// <summary>The glass-shard spawn parameter (the emitter's byte offset into the placement tables) naming each <see cref="MotherBrainGlassShard"/>.</summary>
    internal static readonly NativeWordIdentities<MotherBrainGlassShard> GlassShards = new(new Dictionary<MotherBrainGlassShard, ushort>
    {
        [MotherBrainGlassShard.Right] = 0,
        [MotherBrainGlassShard.Left] = 2,
        [MotherBrainGlassShard.Centre] = 4,
    });

    /// <summary>The bank-$A9 routine word naming each <see cref="MotherBrainTubeAction"/>.</summary>
    internal static readonly NativeWordIdentities<MotherBrainTubeAction> MotherBrainTubeActions = new(new Dictionary<MotherBrainTubeAction, ushort>
    {
        [MotherBrainTubeAction.NonMainTube] = 0x8b88,
        [MotherBrainTubeAction.MainTube_WaitingToFall] = 0x8bcb,
        [MotherBrainTubeAction.MainTube_Falling] = 0x8bd6,
    });

    /// <summary>The bank-$A6 routine word naming each <see cref="ZebetiteAction"/>.</summary>
    internal static readonly NativeWordIdentities<ZebetiteAction> ZebetiteActions = new(new Dictionary<ZebetiteAction, ushort>
    {
        [ZebetiteAction.SpawnLinkedHalf] = 0xfc41,
        [ZebetiteAction.WaitForDoorTransition] = 0xfc5b,
        [ZebetiteAction.Active] = 0xfc67,
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
