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

    /// <summary>
    /// The bank-$A9 routines that draw the mural two rows at a time (#2211), one per pair of rows from the top: the port's
    /// one <see cref="MotherBrainAscentPhase.DrawMural"/> with that many rows drawn.
    /// </summary>
    internal static readonly ushort[] MuralRevealFunctions = [0x8c87, 0x8c9e, 0x8cb5, 0x8ccc, 0x8ce3, 0x8cfa];

    /// <summary>The bank-$A9 routine word of her glass stage, her first phase, which runs no phase of its own (#2208).</summary>
    internal const ushort GlassWord = 0x87e1;

    /// <summary>The bank-$A9 routine word naming each <see cref="MotherBrainDescentPhase"/>, the body's function word (#2208).</summary>
    internal static readonly NativeWordIdentities<MotherBrainDescentPhase> DescentPhases = new(new Dictionary<MotherBrainDescentPhase, ushort>
    {
        [MotherBrainDescentPhase.InitialPause] = 0x881d,
        [MotherBrainDescentPhase.PauseBeforeLock] = 0x8829,
        [MotherBrainDescentPhase.PauseBeforeMusic] = 0x884d,
        [MotherBrainDescentPhase.PauseBeforeUnlock] = 0x886c,
        [MotherBrainDescentPhase.PauseBeforeFlash] = 0x8884,
        [MotherBrainDescentPhase.FadeToGray] = 0x88b2,
        [MotherBrainDescentPhase.CollapseTubes] = 0x88d3,
    });

    /// <summary>The bank-$A9 routine word naming each <see cref="MotherBrainAscentPhase"/>, the body's function word (#2208).</summary>
    internal static readonly NativeWordIdentities<MotherBrainAscentPhase> AscentPhases = new(new Dictionary<MotherBrainAscentPhase, ushort>
    {
        [MotherBrainAscentPhase.DrawMural] = 0x8c87,
        [MotherBrainAscentPhase.SetupPhase2Graphics] = 0x8d11,
        [MotherBrainAscentPhase.SetupPhase2Brain] = 0x8d49,
        [MotherBrainAscentPhase.PauseForSuspense] = 0x8d79,
        [MotherBrainAscentPhase.PrepareForRising] = 0x8d8b,
        [MotherBrainAscentPhase.ReadyStandingBody] = 0x8db4,
        [MotherBrainAscentPhase.ContinuePausing] = 0x8dc3,
        [MotherBrainAscentPhase.StartMusicAndEarthquake] = 0x8dec,
        [MotherBrainAscentPhase.RaiseMotherBrain] = 0x8e4d,
        [MotherBrainAscentPhase.WaitUntilUncrouched] = 0x8e95,
        [MotherBrainAscentPhase.TransitionFromGray] = 0x8eaa,
    });

    /// <summary>The bank-$A9 routine word naming each <see cref="MotherBrainPhase2FightPhase"/>, the body's function word (#2208).</summary>
    internal static readonly NativeWordIdentities<MotherBrainPhase2FightPhase> Phase2FightPhases = new(new Dictionary<MotherBrainPhase2FightPhase, ushort>
    {
        [MotherBrainPhase2FightPhase.StretchingShakeHead] = 0x8ef5,
        [MotherBrainPhase2FightPhase.StretchingBringHeadUp] = 0x8f14,
        [MotherBrainPhase2FightPhase.StretchingFinish] = 0x8f33,
        [MotherBrainPhase2FightPhase.Thinking] = 0xb605,
        [MotherBrainPhase2FightPhase.TryAttack] = 0xb64b,
        [MotherBrainPhase2FightPhase.BombDecideWalking] = 0xb781,
        [MotherBrainPhase2FightPhase.BombWalkingBackwards] = 0xb7ac,
        [MotherBrainPhase2FightPhase.BombCrouch] = 0xb7c6,
        [MotherBrainPhase2FightPhase.BombFired] = 0xb7e8,
        [MotherBrainPhase2FightPhase.BombStandUp] = 0xb7f8,
        [MotherBrainPhase2FightPhase.LaserPositionHeadQuickly] = 0xb80e,
        [MotherBrainPhase2FightPhase.LaserPositionHeadSlowlyAndFire] = 0xb839,
        [MotherBrainPhase2FightPhase.LaserFinishAttack] = 0xb863,
        [MotherBrainPhase2FightPhase.HandBeam] = 0xb87d,
    });

    /// <summary>The bank-$A9 routine word naming each <see cref="MotherBrainRainbowPhase"/>, the body's function word (#2208).</summary>
    internal static readonly NativeWordIdentities<MotherBrainRainbowPhase> RainbowPhases = new(new Dictionary<MotherBrainRainbowPhase, ushort>
    {
        [MotherBrainRainbowPhase.ExtendNeck] = 0xb8eb,
        [MotherBrainRainbowPhase.StartCharging] = 0xb91a,
        [MotherBrainRainbowPhase.RetractNeck] = 0xb92b,
        [MotherBrainRainbowPhase.WaitForCharge] = 0xb93f,
        [MotherBrainRainbowPhase.ExtendNeckDown] = 0xb951,
        [MotherBrainRainbowPhase.StartFiring] = 0xb975,
        [MotherBrainRainbowPhase.MoveSamusTowardWall] = 0xb9e5,
        [MotherBrainRainbowPhase.OneFrameDelay] = 0xba00,
        [MotherBrainRainbowPhase.StartDrainingSamus] = 0xba27,
        [MotherBrainRainbowPhase.DrainingSamus] = 0xba3c,
        [MotherBrainRainbowPhase.FinishFiring] = 0xba5e,
        [MotherBrainRainbowPhase.LetSamusFall] = 0xbac4,
        [MotherBrainRainbowPhase.WaitForSamusToLand] = 0xbad1,
        [MotherBrainRainbowPhase.LowerHead] = 0xbadd,
        [MotherBrainRainbowPhase.DecideNextAction] = 0xbb06,
    });

    /// <summary>The bank-$A9 routine word naming each <see cref="MotherBrainFinishOffPhase"/>, the body's function word (#2208).</summary>
    internal static readonly NativeWordIdentities<MotherBrainFinishOffPhase> FinishOffPhases = new(new Dictionary<MotherBrainFinishOffPhase, ushort>
    {
        [MotherBrainFinishOffPhase.GetSamusToLowEnergy] = 0xbd45,
        [MotherBrainFinishOffPhase.StandUp] = 0xbd98,
        [MotherBrainFinishOffPhase.Admire] = 0xbda9,
        [MotherBrainFinishOffPhase.ChargeFinalBeam] = 0xbdc1,
        [MotherBrainFinishOffPhase.ReadyBaby] = 0xbdd2,
        [MotherBrainFinishOffPhase.FireFinalBeam] = 0xbded,
        [MotherBrainFinishOffPhase.FinalBeamHolding] = 0xbe1a,
    });

    /// <summary>The bank-$A9 routine word naming each <see cref="MotherBrainDrainedPhase"/>, the body's function word (#2208).</summary>
    internal static readonly NativeWordIdentities<MotherBrainDrainedPhase> DrainedPhases = new(new Dictionary<MotherBrainDrainedPhase, ushort>
    {
        [MotherBrainDrainedPhase.TakenAback] = 0xbe38,
        [MotherBrainDrainedPhase.RegainBalance] = 0xbe5d,
        [MotherBrainDrainedPhase.FiringRainbowBeam] = 0xbe96,
        [MotherBrainDrainedPhase.RainbowBeamRunOut] = 0xbf0e,
        [MotherBrainDrainedPhase.MoveToBackOfRoom] = 0xbf41,
        [MotherBrainDrainedPhase.GoIntoLowPowerMode] = 0xbf56,
        [MotherBrainDrainedPhase.PrepareTransitionToGrey] = 0xbf7d,
        [MotherBrainDrainedPhase.TransitionToGrey] = 0xbf95,
    });

    /// <summary>The bank-$A9 routine word naming each <see cref="MotherBrainRevivalPhase"/>, the body's function word (#2208).</summary>
    internal static readonly NativeWordIdentities<MotherBrainRevivalPhase> RevivalPhases = new(new Dictionary<MotherBrainRevivalPhase, ushort>
    {
        [MotherBrainRevivalPhase.InanimateGrey] = 0xc059,
        [MotherBrainRevivalPhase.ShowSignsOfLife] = 0xc066,
        [MotherBrainRevivalPhase.TransitionFromGrey] = 0xc08f,
        [MotherBrainRevivalPhase.WakeUp] = 0xc0ba,
        [MotherBrainRevivalPhase.WakeUpStretch] = 0xc0e4,
        [MotherBrainRevivalPhase.WalkUpToBaby] = 0xc0fb,
        [MotherBrainRevivalPhase.PrepareNeckForBabyDeath] = 0xc11e,
        [MotherBrainRevivalPhase.FinishPreparingForBabyDeath] = 0xc147,
        [MotherBrainRevivalPhase.MurderBabyAttack] = 0xc15c,
        [MotherBrainRevivalPhase.MurderBabyAttackCooldown] = 0xc182,
        [MotherBrainRevivalPhase.PrepareForFinalBabyAttack] = 0xc18e,
        [MotherBrainRevivalPhase.ExecuteFinalBabyAttack] = 0xc19a,
        [MotherBrainRevivalPhase.FinalBabyAttackHolding] = 0xc1a6,
    });

    /// <summary>The bank-$A9 routine word naming each <see cref="MotherBrainPhase3FightPhase"/>, the body's function word (#2208).</summary>
    internal static readonly NativeWordIdentities<MotherBrainPhase3FightPhase> Phase3FightPhases = new(new Dictionary<MotherBrainPhase3FightPhase, ushort>
    {
        [MotherBrainPhase3FightPhase.RecoverMakeSomeDistance] = 0xc1cf,
        [MotherBrainPhase3FightPhase.RecoverSetupForFighting] = 0xc1f0,
        [MotherBrainPhase3FightPhase.FightingMain] = 0xc209,
        [MotherBrainPhase3FightPhase.FightingAttackCooldown] = 0xc24e,
    });

    /// <summary>The bank-$A9 routine word naming each <see cref="MotherBrainDeathPhase"/>, the body's function word (#2208).</summary>
    internal static readonly NativeWordIdentities<MotherBrainDeathPhase> DeathPhases = new(new Dictionary<MotherBrainDeathPhase, ushort>
    {
        [MotherBrainDeathPhase.MoveToBackOfRoom] = 0xaee1,
        [MotherBrainDeathPhase.IdleWhilstExploding] = 0xaf12,
        [MotherBrainDeathPhase.StumbleToMiddleOfRoom] = 0xaf21,
        [MotherBrainDeathPhase.DisableBrainEffects] = 0xaf54,
        [MotherBrainDeathPhase.SetupBodyFadeOut] = 0xaf9d,
        [MotherBrainDeathPhase.FadeOutBody] = 0xafb6,
        [MotherBrainDeathPhase.FinalFewExplosions] = 0xb013,
        [MotherBrainDeathPhase.RealizeDecapitation] = 0xb115,
        [MotherBrainDeathPhase.BrainFallsToGround] = 0xb12d,
        [MotherBrainDeathPhase.ReadyCorpse] = 0xb15e,
        [MotherBrainDeathPhase.SetupFadeToGrey] = 0xb173,
        [MotherBrainDeathPhase.FadeToGrey] = 0xb189,
        [MotherBrainDeathPhase.CorpseTipsOver] = 0xb1b8,
        [MotherBrainDeathPhase.CorpseRotsAway] = 0xb1d5,
        [MotherBrainDeathPhase.PauseBeforeEscape] = 0xb211,
        [MotherBrainDeathPhase.ReadyEscapeWarning] = 0xb258,
        [MotherBrainDeathPhase.StartEscape] = 0xb26d,
        [MotherBrainDeathPhase.SpawnTimeBombSetSubtitle] = 0xb2d1,
        [MotherBrainDeathPhase.TypeOutZebesEscapeText] = 0xb2e3,
        [MotherBrainDeathPhase.DoorExplodingStartTimer] = 0xb2f9,
        [MotherBrainDeathPhase.BlowUpEscapeDoor] = 0xb32a,
        [MotherBrainDeathPhase.KeepEarthquakeGoing] = 0xb33c,
    });

    /// <summary>
    /// Installs the body phase a recorded bank-$A9 routine word names, entering the stage whose phase it is (#2208): one
    /// of the mural's routines enters the ascent's drawing with the rows it has drawn, and the glass stage's word requires
    /// the encounter still in it, the stage it begins in.
    /// </summary>
    /// <exception cref="InvalidDataException">The word names no phase of her body's.</exception>
    internal static void ImportBodyPhase(MotherBrainEnemyState state, ushort word)
    {
        if (word == GlassWord)
        {
            if (state.Stage != MotherBrainBodyStage.Glass)
                throw new InvalidDataException($"A recorded glass-stage Mother Brain cannot be imported over her {state.Stage} stage.");
            return;
        }
        if (Array.IndexOf(MuralRevealFunctions, word) is var pair and >= 0)
        {
            state.AscentPhase = MotherBrainAscentPhase.DrawMural;
            state.NextMuralRow = ListStep<MotherBrainMuralRow>.At(MotherBrainMuralRow.All, pair * MotherBrainMuralDefinitions.RowsEachUpdate);
            return;
        }
        state.NextMuralRow = MotherBrainMuralRow.First;
        if (DescentPhases.TryDecode(word, out MotherBrainDescentPhase descentPhase))
            state.DescentPhase = descentPhase;
        else if (Phase2FightPhases.TryDecode(word, out MotherBrainPhase2FightPhase phase2FightPhase))
            state.Phase2FightPhase = phase2FightPhase;
        else if (RainbowPhases.TryDecode(word, out MotherBrainRainbowPhase rainbowPhase))
            state.RainbowPhase = rainbowPhase;
        else if (FinishOffPhases.TryDecode(word, out MotherBrainFinishOffPhase finishOffPhase))
            state.FinishOffPhase = finishOffPhase;
        else if (DrainedPhases.TryDecode(word, out MotherBrainDrainedPhase drainedPhase))
            state.DrainedPhase = drainedPhase;
        else if (RevivalPhases.TryDecode(word, out MotherBrainRevivalPhase revivalPhase))
            state.RevivalPhase = revivalPhase;
        else if (Phase3FightPhases.TryDecode(word, out MotherBrainPhase3FightPhase phase3FightPhase))
            state.Phase3FightPhase = phase3FightPhase;
        else if (DeathPhases.TryDecode(word, out MotherBrainDeathPhase deathPhase))
            state.DeathPhase = deathPhase;
        else
            state.AscentPhase = AscentPhases.Decode(word, "recorded Mother Brain body phase");
    }

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

    /// <summary>
    /// The wrong-way speed, in 256ths of a pixel an update, each bank-$A9 movement routine word a Baby Metroid route record
    /// names reverses with: the gradual accelerations with the extra eight and the extra sixteen.
    /// </summary>
    internal static readonly IReadOnlyDictionary<ushort, int> BabyMetroidRouteWrongWaySpeeds = new Dictionary<ushort, int>
    {
        [0xf45f] = 8,
        [0xf466] = 16,
    };

    /// <summary>
    /// The bank-$A9 routine word naming each <see cref="ShitroidAction"/> and, for its departing flight, the side of Samus it
    /// flies to (#2165), the cartridge giving each side a routine of its own.
    /// </summary>
    internal static readonly NativeWordIdentities<(ShitroidAction Action, HorizontalDirection? Side)> ShitroidActions =
        new(new Dictionary<(ShitroidAction Action, HorizontalDirection? Side), ushort>
    {
        [(ShitroidAction.Dormant, null)] = 0xefdf,
        [(ShitroidAction.WaitForCamera, null)] = 0xefe6,
        [(ShitroidAction.BeginEntranceDelay, null)] = 0xf02b,
        [(ShitroidAction.EntranceDelay, null)] = 0xf037,
        [(ShitroidAction.FlyToSidehopper, null)] = 0xf049,
        [(ShitroidAction.ChaseSidehopper, null)] = 0xf06d,
        [(ShitroidAction.AttachToSidehopper, null)] = 0xf094,
        [(ShitroidAction.DrainSidehopper, null)] = 0xf0e6,
        [(ShitroidAction.ActivateSidehopperCorpse, null)] = 0xf125,
        [(ShitroidAction.RiseAfterFeeding, null)] = 0xf138,
        [(ShitroidAction.HoverNearSamus, null)] = 0xf180,
        [(ShitroidAction.ChaseSamus, null)] = 0xf1fa,
        [(ShitroidAction.BeginDrainingSamus, null)] = 0xf20e,
        [(ShitroidAction.DrainSamus, null)] = 0xf21b,
        [(ShitroidAction.BeginPostDrainPause, null)] = 0xf2a2,
        [(ShitroidAction.PostDrainPause, null)] = 0xf2ae,
        [(ShitroidAction.RiseAfterDrainingSamus, null)] = 0xf2c0,
        [(ShitroidAction.FlyBesideSamus, HorizontalDirection.Left)] = 0xf2fb,
        [(ShitroidAction.FlyBesideSamus, HorizontalDirection.Right)] = 0xf324,
        [(ShitroidAction.BeginExit, null)] = 0xf360,
        [(ShitroidAction.Exit, null)] = 0xf36d,
        [(ShitroidAction.HoldSamusBeforeRelease, null)] = 0xf3a3,
        [(ShitroidAction.ReleasedFollow, null)] = 0xf3be,
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
        [MotherBrainTubeAction.WaitingToFall] = 0x8bcb,
        [MotherBrainTubeAction.Falling] = 0x8bd6,
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
    internal static ZebetiteBarrierForm DecodeBarrierForm(ushort word) => word switch
    {
        0 => ZebetiteBarrierForm.Single,
        LinkedPairWord => ZebetiteBarrierForm.SplitPair,
        _ => throw new InvalidDataException($"${word:X4} is not a Zebetite generation word."),
    };

    /// <summary>Encodes a Zebetite generation's form as the cartridge's variable F word.</summary>
    internal static ushort BarrierFormWord(ZebetiteBarrierForm form) => form switch
    {
        ZebetiteBarrierForm.Single => 0,
        ZebetiteBarrierForm.SplitPair => LinkedPairWord,
        _ => throw new ArgumentOutOfRangeException(nameof(form), form, "Undefined Zebetite barrier form."),
    };
}
