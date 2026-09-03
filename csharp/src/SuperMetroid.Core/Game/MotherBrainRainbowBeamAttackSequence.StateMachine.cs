using SuperMetroid.Core.Hardware;

using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The phase dispatcher and its exact fall-through transitions.
/// </summary>
public sealed partial class MotherBrainRainbowBeamAttackSequence
{
    /// <summary>Executes one call through the current Mother Brain body function.</summary>
    public MotherBrainRainbowBeamAttackStepResult Step(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort enemyFrameCounter,
        ushort mainEnemyExecutionCounter,
        bool powerBombActive = false,
        ushort randomNumberSeed = 0,
        Func<ushort>? nextRandomNumber = null,
        bool alternateEscapeText = false,
        bool typewriterFinished = false,
        ushort? globalEarthquakeTimer = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        if (Phase == MotherBrainRainbowBeamAttackPhase.Inactive)
            throw new InvalidOperationException("The active rainbow-beam sequence has not started.");

        MotherBrainRainbowBeamAttackPhase phaseBefore = Phase;
        uint headInstructionListRequestSerialBefore = _headInstructionListRequestSerial;
        MotherBrainForcedSamusMovementResult? movement = null;
        bool soundQueued = false;
        bool paletteRequested = false;
        MotherBrainRainbowExplosionRequest? explosion = null;
        ushort healthBefore = samus.Health;
        ushort missilesBefore = samus.Missiles;
        ushort supersBefore = samus.SuperMissiles;
        ushort bombsBefore = samus.PowerBombs;
        bool unlockedSamus = false;
        bool chargeSoundQueued = false;
        bool bodyWalkRequested = false;
        bool bodyPostureRequested = false;
        MotherBrainFinishOffAttackKind? finishOffAttack = null;
        MotherBrainSpriteTileTransferRequest? spriteTileTransfer = null;
        bool babySpawnRequested = false;
        bool finalBeamSoundQueued = false;
        MotherBrainPhase3AttackKind? phase3Attack = null;
        var deathExplosions = new List<MotherBrainDeathExplosionRequest>(capacity: 4);
        var corpseRottingVramTransfers =
            new List<MotherBrainSpriteTileTransferRequest>(capacity: 6);
        var corpseDustRequests = new List<MotherBrainCorpseDustRequest>(capacity: 1);
        bool musicStopQueued = false;
        bool escapeMusicQueued = false;
        var escapeSequenceTileTransfers =
            new List<MotherBrainSpriteTileTransferRequest>(capacity: 2);
        bool explodedDoorPaletteRequested = false;
        bool escapeMusicTrackQueued = false;
        var escapePaletteFxRequests = new List<ushort>(capacity: 4);
        bool escapeTypewriterSetupRequested = false;
        bool typewriterStepRequested = false;
        ushort? typewriterTextPointer = null;
        bool timeBombSetSubtitleSpawnRequested = false;
        MotherBrainEscapeDoorExplosionRequest? escapeDoorExplosion = null;
        bool timerHandlingEnableRequested = false;
        bool motherBrainEscapeTimerStartRequested = false;
        bool motherBrainBossBitRequested = false;
        bool zebesTimebombEventRequested = false;
        var escapeDoorParticleSpawns =
            new List<MotherBrainEscapeDoorParticleSpawnRequest>(capacity: 8);
        MotherBrainEscapeDoorPlmRequest? escapeDoorPlm = null;
        bool earthquakeTimerRefreshed = false;

        switch (Phase)
        {
            case MotherBrainRainbowBeamAttackPhase.RepeatAttack:
                // `$A9:BB13` only installs `$B8EB`; the setup executes on the next enemy
                // AI call and deliberately does not fall through into its 256 timer.
                BeginExtendingNeckForAttack();
                break;

            case MotherBrainRainbowBeamAttackPhase.StartCharging:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    SetHeadInstructionList(HeadChargingRainbowInstructionList);
                    Phase = MotherBrainRainbowBeamAttackPhase.RetractNeck;
                    goto case MotherBrainRainbowBeamAttackPhase.RetractNeck;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.RetractNeck:
                bodyWalkRequested = RequestWalkBackwardReallySlow(targetX: 0x0028);
                if (HasReachedBackwardTarget(targetX: 0x0028))
                {
                    RetractHead();
                    Phase = MotherBrainRainbowBeamAttackPhase.WaitForCharge;
                    FunctionTimer = 0x0100;
                    goto case MotherBrainRainbowBeamAttackPhase.WaitForCharge;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.WaitForCharge:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    chargeSoundQueued = true; // Sound library two, effect `$71`.
                    Phase = MotherBrainRainbowBeamAttackPhase.ExtendNeckDown;
                    goto case MotherBrainRainbowBeamAttackPhase.ExtendNeckDown;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.ExtendNeckDown:
                SamusProjectileCooldownTimer = 8;
                LowerNeckMovementIndex = 6;
                UpperNeckMovementIndex = 6;
                NeckAngleDelta = 0x0500; // NTSC value of regional `$0500/$0700`.
                Phase = MotherBrainRainbowBeamAttackPhase.StartFiring;
                FunctionTimer = 0x0010; // NTSC value of regional `$0010/$000C`.
                goto case MotherBrainRainbowBeamAttackPhase.StartFiring;

            case MotherBrainRainbowBeamAttackPhase.StartFiring:
                // `$A9:B975` widens/aims even while an active power bomb freezes the timer.
                IncreaseWidthAndAim(samus);
                if (!powerBombActive)
                {
                    FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                    if ((FunctionTimer & 0x8000) != 0)
                    {
                        // The cartridge redundantly rereads the power-bomb flag here. The
                        // parameter is one stable WRAM sample for this translated AI call.
                        SamusProjectileCooldownTimer = 0;
                        StartActiveBeam(bus, samus);
                    }
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.MoveSamusTowardWall:
                RunContinuingBeamEffects(
                    samus,
                    enemyFrameCounter,
                    increaseWidth: true,
                    ref soundQueued,
                    ref paletteRequested,
                    ref explosion);
                movement = _movement.MoveTowardWall(bus, samus);
                if (movement.Value.NativeCarry)
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.OneFrameDelay;
                    FunctionTimer = 0;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.OneFrameDelay:
                RunContinuingBeamEffects(
                    samus,
                    enemyFrameCounter,
                    increaseWidth: true,
                    ref soundQueued,
                    ref paletteRequested,
                    ref explosion);
                movement = _movement.MoveTowardWall(bus, samus);

                // `$A9:BA0F` decrements zero to `$FFFF`; BPL fails on this very call.
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    EarthquakeType = 8;
                    EarthquakeTimer = 8;
                    Phase = MotherBrainRainbowBeamAttackPhase.StartDrainingSamus;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.StartDrainingSamus:
                // `$A9:BA27` installs draining, seeds both timers with `$012B`, and falls
                // through into the draining function in the same enemy-processing call.
                Phase = MotherBrainRainbowBeamAttackPhase.DrainingSamus;
                FunctionTimer = 0x012b;
                EarthquakeTimer = 0x012b;
                EarthquakeType = 8;
                goto case MotherBrainRainbowBeamAttackPhase.DrainingSamus;

            case MotherBrainRainbowBeamAttackPhase.DrainingSamus:
                RunContinuingBeamEffects(
                    samus,
                    enemyFrameCounter,
                    increaseWidth: true,
                    ref soundQueued,
                    ref paletteRequested,
                    ref explosion);
                DamageSamusDueToRainbowBeam(samus);
                DecrementAmmoDueToRainbowBeam(samus, mainEnemyExecutionCounter);
                movement = MotherBrainRainbowBeamSamusMovement.MoveTowardMiddleOfWall(samus);

                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                    Phase = MotherBrainRainbowBeamAttackPhase.FinishFiring;
                break;

            case MotherBrainRainbowBeamAttackPhase.FinishFiring:
                RunContinuingBeamEffects(
                    samus,
                    enemyFrameCounter,
                    increaseWidth: false,
                    ref soundQueued,
                    ref paletteRequested,
                    ref explosion);

                // `$A9:BA6A-$BA7E` narrows first, then compares the wrapped signed result
                // with `$0200`. Width is pinned only after it crosses below that floor.
                AngularWidth = unchecked((ushort)(AngularWidth - 0x0180));
                if (!NativeAtLeast(AngularWidth, 0x0200))
                {
                    AngularWidth = 0x0200;
                    _movement.BeginFallingAfterRainbowBeam();
                    HdmaActive = false;
                    EarthquakeTimer = 0;
                    RainbowBeamSoundPlaying = false;
                    samus.InputLocked = false; // Samus command one at `$90:F117`.
                    unlockedSamus = true;
                    SamusProjectileCooldownTimer = 8;
                    Phase = MotherBrainRainbowBeamAttackPhase.LetSamusFall;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.LetSamusFall:
                // Controller zero changes the pose/radius, then `$A9:BACB` installs the wait
                // function and falls straight through to its first custom movement call.
                samus.Drained.LetFall(bus, samus);
                Phase = MotherBrainRainbowBeamAttackPhase.WaitForSamusToLand;
                goto case MotherBrainRainbowBeamAttackPhase.WaitForSamusToLand;

            case MotherBrainRainbowBeamAttackPhase.WaitForSamusToLand:
                movement = _movement.StepFallingAfterRainbowBeam(samus);
                if (movement.Value.NativeCarry)
                    Phase = MotherBrainRainbowBeamAttackPhase.LowerHead;
                break;

            case MotherBrainRainbowBeamAttackPhase.LowerHead:
                // Neck words are presentation/actor-animation state. This phase's movement-
                // relevant effect is the exact function/timer handoff to `$A9:BB06`.
                Phase = MotherBrainRainbowBeamAttackPhase.DecideNextAction;
                FunctionTimer = 0x0080;
                break;

            case MotherBrainRainbowBeamAttackPhase.DecideNextAction:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    if (NativeAtLeast(samus.Health, 0x0190))
                    {
                        Phase = MotherBrainRainbowBeamAttackPhase.RepeatAttack;
                    }
                    else
                    {
                        // `$A9:BB1A-$BB24` performs this call before installing the later
                        // finish-off function. It starts one complete really-slow forward
                        // body animation when Mother Brain is standing and left of `$80`.
                        bodyWalkRequested = RequestWalkForwardReallySlow(
                            unchecked((ushort)(Body.XPosition + 0x0010)));
                        BabyMetroidTileTransferIndex = 0;
                        BabyMetroidSpawned = false;
                        Phase = MotherBrainRainbowBeamAttackPhase.FinishSamusOff;
                    }
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.FinishSamusOff:
                // `$A9:BD45-$BD54` asks whether one more ordinary phase-two hit could put
                // Samus at the encounter's intended low-energy floor. The first nominal
                // damage is `$50`, suit-divided, multiplied by four, then padded by twenty.
                // The native BPL comparison treats equality as done and changes the body
                // function without falling into the stand-up handler on this call.
                if (NativeAtLeast(CalculateFinishOffHealthThreshold(samus, 0x0050), samus.Health))
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.FinishStandUp;
                    break;
                }

                // Above the floor, 4000/4096 low-twelve-bit RNG values do nothing. Every
                // 32nd enemy frame that idle route may ask the posture helper to alternate
                // between standing and leaning down. This is visible body bytecode, not a
                // direct pose assignment, so retain the separate instruction-stage delay.
                if ((randomNumberSeed & 0x0fff) < 0x0fa0)
                {
                    if ((enemyFrameCounter & 0x001f) == 0)
                        bodyPostureRequested = MaybeRequestStandUpOrLeanDown(randomNumberSeed);
                    break;
                }

                // A weaker `$A0` nominal-hit threshold biases wounded Samus toward onion
                // rings. Only when Samus is above it does the top 16/4096 RNG band choose a
                // bomb; all remaining admitted values still choose two onion rings.
                if (NativeAtLeast(CalculateFinishOffHealthThreshold(samus, 0x00a0), samus.Health) ||
                    (randomNumberSeed & 0x0fff) < 0x0ff0)
                {
                    SetHeadInstructionList(HeadAttackingTwoOnionRingsPhase2InstructionList);
                    finishOffAttack = MotherBrainFinishOffAttackKind.TwoOnionRings;
                }
                else
                {
                    SetHeadInstructionList(HeadAttackingBombPhase2InstructionList);
                    finishOffAttack = MotherBrainFinishOffAttackKind.Bomb;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.FinishStandUp:
                // `$A9:C670` reports carry only if pose was already zero at call entry.
                // Crouched/leaning poses request their matching stand animation and return
                // clear; walking/transition poses simply wait for their current bytecode.
                if (MakeBodyStandUp(out bodyPostureRequested))
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.AdmireJobWellDone;
                    FunctionTimer = 0x0010;
                    goto case MotherBrainRainbowBeamAttackPhase.AdmireJobWellDone;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.AdmireJobWellDone:
                // Stand-up falls through here, so the freshly written `$10` becomes `$0F`
                // on the same AI call. BPL accepts zero and expires only after underflow.
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    SetHeadInstructionList(HeadStretchingPhase2InstructionList);
                    Phase = MotherBrainRainbowBeamAttackPhase.ChargeFinalRainbowBeam;
                    FunctionTimer = 0x0100;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.ChargeFinalRainbowBeam:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    SetHeadInstructionList(HeadChargingRainbowInstructionList);
                    Phase = MotherBrainRainbowBeamAttackPhase.LoadBabyMetroidTiles;
                    goto case MotherBrainRainbowBeamAttackPhase.LoadBabyMetroidTiles;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.LoadBabyMetroidTiles:
                // `$A9:C5BE` processes exactly one seven-byte table entry per call. The
                // fourth entry sees the following zero terminator, clears its saved pointer,
                // and returns carry set on that same call.
                spriteTileTransfer = CreateNextBabyMetroidTileTransfer();
                if (BabyMetroidTileTransferIndex == BabyMetroidTileSources.Length)
                {
                    RetractHead();
                    BabyMetroidSpawned = true;
                    babySpawnRequested = true;
                    Phase = MotherBrainRainbowBeamAttackPhase.FireFinalRainbowBeam;
                    FunctionTimer = 0x0100;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.FireFinalRainbowBeam:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    RainbowBeamPaletteAnimationIndex = 0;
                    SetHeadInstructionList(HeadFiringRainbowInstructionList);
                    LowerNeckMovementIndex = 6;
                    UpperNeckMovementIndex = 6;
                    NeckAngleDelta = 0x0500;
                    finalBeamSoundQueued = true; // Sound library two, effect `$71`.
                    Phase = MotherBrainRainbowBeamAttackPhase.FinalRainbowBeamHolding;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.FinalRainbowBeamHolding:
                // `$A9:BE14` stores the address of its own RTS. Mother Brain remains here
                // until the independently spawned Baby actor overwrites her body function
                // with `$BE38` after latching onto her head.
                break;

            case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTakenAback:
                // `$A9:BE38` is not executed by the Baby. It runs on Mother Brain's next
                // actor turn, changes form/neck geometry, seeds `$30`, and falls through so
                // that this first regain-balance call immediately decrements it to `$2F`.
                Body.Form = 3;
                LowerNeckMovementIndex = 8;
                UpperNeckMovementIndex = 8;
                NeckAngleDelta = 0x0700;
                Phase = MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRegainBalance;
                FunctionTimer = 0x0030;
                goto case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRegainBalance;

            case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRegainBalance:
                paletteRequested = true;
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    // `$BE80-$BE83` contains an apparent missing STA: it loads one and then
                    // immediately reloads the neck-enable flag. Preserve that no-op rather
                    // than silently "fixing" the cartridge.
                    PainfulWalkingForward = true;
                    PainfulWalkingStage = 0;
                    PainfulWalkingAnimationDelay = 2;
                    PainfulWalkingFunctionTimer = 0;
                    LowerNeckMovementIndex = 2;
                    UpperNeckMovementIndex = 4;
                    Phase = MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidFiringRainbowBeam;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidFiringRainbowBeam:
                // `$BE96` refreshes the shake timer only if another subsystem has cleared it.
                // The head animation owns any decrement; retaining the write gate avoids
                // fabricating a body-side countdown that does not exist in this routine.
                if (BrainMainShakeTimer == 0)
                    BrainMainShakeTimer = 0x0032;
                paletteRequested = true;
                bodyWalkRequested = StepPainfulWalking();

                // The outer function updates delay and neck delta after *every* nested call.
                // Stage six is therefore observed on the exact call which ends its prior
                // pause, before the stage-six forward walk has even begun.
                if (PainfulWalkingStage < PainfulWalkingAnimationDelays.Length)
                {
                    PainfulWalkingAnimationDelay =
                        PainfulWalkingAnimationDelays[PainfulWalkingStage];
                    NeckAngleDelta = PainfulWalkingNeckAngleDeltas[PainfulWalkingStage];
                }
                if (PainfulWalkingStage == 6)
                {
                    RainbowBeamSoundPlaying = false;
                    BrainPaletteHandlingEnabled = false;
                    Phase = MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRainbowBeamRunOut;
                    soundQueued = true; // Sound library one, effect `$02`.
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRainbowBeamRunOut:
                bodyWalkRequested = StepPainfulWalking();
                if (PainfulWalkingStage >= 8)
                {
                    NeckAngleDelta = 0x0040;
                    LowerNeckMovementIndex = 8;
                    UpperNeckMovementIndex = 8;
                    SetHeadInstructionList(HeadDyingDroolInstructionList);
                    Phase = MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidMoveToBackOfRoom;
                    goto case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidMoveToBackOfRoom;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidMoveToBackOfRoom:
                // `$BF41` does not reload Y before the shared walk helper. The surrounding
                // sequence's final painful-animation selector is `$000A`, so the observable
                // retail list is the really-slow backward program at `$993A`.
                bodyWalkRequested = RequestWalkBackwardReallySlow(targetX: 0x0028);
                if (HasReachedBackwardTarget(targetX: 0x0028))
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidGoIntoLowPowerMode;
                    UpperNeckMovementIndex = 0;
                    goto case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidGoIntoLowPowerMode;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidGoIntoLowPowerMode:
                // The brain slot independently advances the one-way neck raises. The body
                // must wait for both indices and its walk pose to return to zero.
                if ((LowerNeckMovementIndex | UpperNeckMovementIndex) == 0 && Body.Pose == 0)
                {
                    DroolGenerationEnabled = false;
                    Body.SetInstructionList(BodyCrouchingFastInstructionList);
                    bodyPostureRequested = true;
                    Phase = MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidPrepareTransitionToGrey;
                    FunctionTimer = 0x0040;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidPrepareTransitionToGrey:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    GreyTransitionCounter = 0;
                    Phase = MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTransitionToGrey;
                    FunctionTimer = 0x0010;
                    goto case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTransitionToGrey;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTransitionToGrey:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    FunctionTimer = 0x0010;

                    // Bank `$AD:EF4A` has eight nonzero palette pointers followed by zero.
                    // The caller increments its counter, passes the old index, and treats
                    // the ninth (index-eight) probe as completion without copying colours.
                    ushort paletteIndex = GreyTransitionCounter;
                    GreyTransitionCounter++;
                    paletteRequested = paletteIndex < 8;
                    if (paletteIndex >= 8)
                    {
                        BrainHealth = 0x8ca0;
                        Phase2CorpseState = 1;
                        SmallPurpleBreathGenerationEnabled = false;
                        Body.Form = 2;
                        Phase = MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfInanimateGrey;
                    }
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfInanimateGrey:
                // `$C059` only installs the next function and its `$0300` timer. It does not
                // fall through, so the first pre-decrement belongs to the following call.
                Phase = MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfShowSignsOfLife;
                FunctionTimer = 0x0300;
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfShowSignsOfLife:
                // BPL accepts zero: 768 reaches zero without advancing and only the 769th
                // call underflows to `$FFFF`. Revival then re-enables both breath producers,
                // seeds `$E0`, and falls through two function installations to `$C08F`.
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    SmallPurpleBreathGenerationEnabled = true;
                    DroolGenerationEnabled = true;
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfTransitionFromGrey;
                    FunctionTimer = 0x00e0;
                    GreyTransitionCounter = 0;
                    goto case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfTransitionFromGrey;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfTransitionFromGrey:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    FunctionTimer = 0x0010;

                    // `$AD:ED9C` reverses the same eight grey palettes used by the corpse
                    // transition and follows them with zero. The body counter is incremented
                    // before the old index is passed, so index eight is a terminating probe
                    // with no copy. The existing result flag exposes each real copy cadence.
                    ushort paletteIndex = GreyTransitionCounter;
                    GreyTransitionCounter++;
                    paletteRequested = paletteIndex < 8;
                    if (paletteIndex >= 8)
                    {
                        BrainPaletteHandlingEnabled = true;
                        Phase = MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWakeUp;
                        goto case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWakeUp;
                    }
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWakeUp:
                // `$C670` returns carry only when pose zero was already visible at entry.
                // A corpse pose therefore installs `$99C6` and waits for the ordinary body
                // instruction stage to publish standing before setting the neck/timer words.
                if (MakeBodyStandUp(out bodyPostureRequested))
                {
                    LowerNeckMovementIndex = 6;
                    UpperNeckMovementIndex = 6;
                    NeckAngleDelta = 0x0500;
                    NeckMovementEnabled = 1;
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWakeUpStretch;
                    FunctionTimer = 0x0010;
                    goto case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWakeUpStretch;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWakeUpStretch:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    SetHeadInstructionList(HeadStretchingPhase3InstructionList);
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWalkUpToBabyMetroid;
                    FunctionTimer = 0x0080;
                    goto case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWalkUpToBabyMetroid;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWalkUpToBabyMetroid:
                // Once `$80` underflows, the timer deliberately remains negative and the
                // helper is retried after every body-program call until X has overshot `$50`.
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    bodyWalkRequested = RequestWalkForward(0x0050, 0x0004);
                    bool reachedWalkTarget = unchecked((short)(0x0050 - Body.XPosition)) < 0 ||
                        NativeAtLeast(Body.XPosition, 0x0080);
                    if (reachedWalkTarget)
                    {
                        Phase2CorpseState = 2;
                        HealthBasedPaletteHandlingEnabled = true;
                        Phase = MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfPrepareNeckForBabyMetroidDeath;
                    }
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfPrepareNeckForBabyMetroidDeath:
                // `$C11E` is a one-call setup function which immediately falls into `$C147`.
                // The separate body instruction stage may still be finishing the last walk.
                BabyMetroidAttackCounter = 0;
                NeckMovementEnabled = 1;
                LowerNeckMovementIndex = 2;
                UpperNeckMovementIndex = 4;
                NeckAngleDelta = 0x0040;
                Phase = MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfFinishPreparingForBabyMetroidDeath;
                goto case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfFinishPreparingForBabyMetroidDeath;

            case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfFinishPreparingForBabyMetroidDeath:
                if (MakeBodyStandUp(out bodyPostureRequested))
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase2MurderBabyMetroidAttack;
                    bodyWalkRequested = RequestWalkForward(0x0050, 0x000a);
                    goto case MotherBrainRainbowBeamAttackPhase.Phase2MurderBabyMetroidAttack;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase2MurderBabyMetroidAttack:
                bodyPostureRequested = MaybeRequestStandUpOrLeanDown(randomNumberSeed);
                if ((randomNumberSeed & 0x8000) != 0)
                {
                    // A nonzero Baby enemy index selects `$9DB1`; zero instead targets Samus.
                    // This translated cutscene owns a spawned Baby, so retain the live actor
                    // condition instead of unconditionally substituting the desired list.
                    SetHeadInstructionList(BabyMetroidSpawned
                        ? HeadAttackingBabyMetroidInstructionList
                        : (ushort)0x9dbb);
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase2MurderBabyMetroidAttackCooldown;
                    FunctionTimer = 0x0040;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase2MurderBabyMetroidAttackCooldown:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase2MurderBabyMetroidAttack;
                break;

            case MotherBrainRainbowBeamAttackPhase.PrepareForFinalBabyMetroidAttack:
                // This native function has no state transition. It requests stand-up and a
                // fast backward walk toward `$40` every call until the Baby overwrites it.
                MakeBodyStandUp(out bodyPostureRequested);
                bodyWalkRequested |= RequestWalkBackward(0x0040, 0x0004);
                break;

            case MotherBrainRainbowBeamAttackPhase.ExecuteFinalBabyMetroidAttack:
                SetHeadInstructionList(HeadAttackingBabyMetroidInstructionList);
                Phase = MotherBrainRainbowBeamAttackPhase.FinalBabyMetroidAttackHolding;
                break;

            case MotherBrainRainbowBeamAttackPhase.FinalBabyMetroidAttackHolding:
                // `$C1A6` is the installed RTS. The last ring volley owns the remaining
                // health reduction and the Baby actor owns its death/recovery sequence.
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3RecoverFromCutsceneMakeSomeDistance:
                // `$C1CF` changes form before installing the setup wait. Its backward-walk
                // request uses target X-14 and delay index two; the ordinary helper may
                // legitimately reject it at the arena's hard left limit.
                Body.Form = 4;
                Phase = MotherBrainRainbowBeamAttackPhase.Phase3RecoverFromCutsceneSetupForFighting;
                FunctionTimer = 0x0020;
                bodyWalkRequested = RequestWalkBackward(
                    unchecked((ushort)(Body.XPosition - 0x000e)),
                    animationDelay: 0x0002);
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3RecoverFromCutsceneSetupForFighting:
                // `$20` remains valid through zero and expires only when DEC wraps negative,
                // yielding 33 setup calls after the one-frame `$C1CF` producer above.
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3FightingMain;
                    Phase3NeckPhase = MotherBrainPhase3NeckPhase.Normal;
                    Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.TryToInchForward;

                    // `$C1F0-$C205` has no RTS after installing the three function pointers;
                    // execution falls directly into `$C209` on this same enemy AI call.
                    goto case MotherBrainRainbowBeamAttackPhase.Phase3FightingMain;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3FightingMain:
                // `$C209` tests death before either sub-handler. It installs the translated
                // `$AEE1` explosion/fade sequence without executing that function's property
                // writes until the following body turn; no phase-two behavior leaks through.
                if (BrainHealth == 0)
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceMoveToBackOfRoom;
                    break;
                }

                StepPhase3NeckHandler();
                bodyWalkRequested = StepPhase3WalkingHandler(randomNumberSeed);

                // Walking bytecode changes pose later in the enemy instruction stage. The
                // AI therefore still sees pose zero on the call that requests a walk and may
                // also select an attack, exactly as the source's post-handler `$C21B` load.
                if (Body.Pose != 0 || Phase3DisableAttacks != 0 ||
                    (randomNumberSeed & 0x8000) == 0)
                    break;

                if ((randomNumberSeed & 0x00ff) < 0x0080)
                {
                    SetHeadInstructionList(HeadAttackingBombPhase3InstructionList);
                    phase3Attack = MotherBrainPhase3AttackKind.Bomb;
                }
                else
                {
                    SetHeadInstructionList(HeadAttackingFourOnionRingsPhase3InstructionList);
                    phase3Attack = MotherBrainPhase3AttackKind.FourOnionRings;
                }

                Phase = MotherBrainRainbowBeamAttackPhase.Phase3FightingAttackCooldown;
                FunctionTimer = 0x0040;
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3FightingAttackCooldown:
                // `$C24E` accepts timer zero and returns to main only after underflow. It
                // does not fall through, so neck and walking handlers pause for all 65 calls.
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3FightingMain;
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceMoveToBackOfRoom:
                // `$A9:AEE1` makes both enemy slots non-interactive and disables the shared
                // hitboxes on every call, then reuses the ordinary medium-speed backward
                // walk producer. Carry means X `$28` has been reached/overshot; the new `$80`
                // timer falls through so the first smoky explosion and decrement happen now.
                BodyProperties = BodyProperties.With(EnemyProperties.IgnoreSamusCollision);
                BrainProperties = BrainProperties.With(EnemyProperties.IgnoreSamusCollision);
                HitboxesEnabled = false;
                bodyWalkRequested = RequestWalkBackward(0x0028, animationDelay: 0x0006);
                if (HasReachedBackwardTarget(0x0028))
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceIdleWhilstExploding;
                    FunctionTimer = 0x0080;
                    goto case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceIdleWhilstExploding;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceIdleWhilstExploding:
                GenerateDeathExplosions(
                    mixed: false, nextRandomNumber, deathExplosions);
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceStumbleToMiddleOfRoom;
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceStumbleToMiddleOfRoom:
                // `$AF21` emits smoke before asking for a really-fast forward walk to `$60`.
                // Equality still starts a complete body program; only signed overshoot or the
                // independent `$80` arena clamp reports carry and advances the death script.
                GenerateDeathExplosions(
                    mixed: false, nextRandomNumber, deathExplosions);
                bodyWalkRequested = RequestWalkForward(0x0060, animationDelay: 0x0002);
                if (unchecked((short)(0x0060 - Body.XPosition)) < 0 ||
                    NativeAtLeast(Body.XPosition, 0x0080))
                {
                    SetHeadInstructionList(HeadDyingDroolInstructionList);
                    LowerNeckMovementIndex = 6;
                    UpperNeckMovementIndex = 6;
                    NeckAngleDelta = 0x0500;
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceDisableBrainEffects;
                    FunctionTimer = 0x0020;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceDisableBrainEffects:
                GenerateDeathExplosions(
                    mixed: false, nextRandomNumber, deathExplosions);
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    // `$AF5C-$AF94` removes the neck/breath/palette producers, copies sprite
                    // palette 1 into palette 7, performs one health-palette refresh, and then
                    // deliberately falls through with the already-negative timer.
                    LowerNeckMovementIndex = 0;
                    UpperNeckMovementIndex = 0;
                    DroolGenerationEnabled = false;
                    SmallPurpleBreathGenerationEnabled = false;
                    BrainPaletteHandlingEnabled = false;
                    HealthBasedPaletteHandlingEnabled = false;
                    BrainPaletteIndex = EnemyPaletteBits.Palette7;
                    paletteRequested = true;
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSetupBodyFadeOut;
                    goto case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSetupBodyFadeOut;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSetupBodyFadeOut:
                GenerateDeathExplosions(
                    mixed: true, nextRandomNumber, deathExplosions);
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    GreyTransitionCounter = 0;
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeOutBody;
                    FunctionTimer = 0;
                    goto case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeOutBody;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeOutBody:
                // Body flickering runs every AI call, independently of the seventeen-call
                // palette cadence. Counter values 0..15 copy blackening palettes; value 16
                // is the null terminator and completes the transition without a copy.
                BodyFlickerCallCount++;
                GenerateDeathExplosions(
                    mixed: true, nextRandomNumber, deathExplosions);
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    FunctionTimer = 0x0010;
                    ushort paletteIndex = GreyTransitionCounter;
                    GreyTransitionCounter++;
                    paletteRequested |= paletteIndex < 16;
                    if (paletteIndex >= 16)
                    {
                        EnemyBg2TilemapClearRequested = true;
                        BodyProperties = BodyProperties
                            .With(EnemyProperties.Invisible)
                            .Without(EnemyProperties.ProcessInstructions);
                        BodyProperties2 = 0;
                        Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFinalFewExplosions;
                        FunctionTimer = 0x0010;
                    }
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFinalFewExplosions:
                GenerateDeathExplosions(
                    mixed: true, nextRandomNumber, deathExplosions);
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceRealizeDecapitation;
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceRealizeDecapitation:
                // The body installs the decapitated list and a separate brain-slot draw
                // setup, clears its 8.8 falling velocity, then falls through immediately.
                SetHeadInstructionList(HeadDecapitatedInstructionList);
                BrainDrawSetupRequested = true;
                FunctionTimer = 0;
                Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceBrainFallsToGround;
                goto case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceBrainFallsToGround;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceBrainFallsToGround:
                // `$B12D` treats FunctionTimer as unsigned 8.8 velocity: add `$20`, use only
                // its high byte as this frame's whole-pixel displacement, and accumulate Y.
                // Crossing `$C4` clamps the head and publishes earthquake type two for 20.
                FunctionTimer = unchecked((ushort)(FunctionTimer + 0x0020));
                ushort fallingDisplacement = (ushort)(FunctionTimer >> 8);
                ushort candidateBrainY = unchecked((ushort)(BrainYPosition + fallingDisplacement));
                if (candidateBrainY >= 0x00c4)
                {
                    EarthquakeType = 2;
                    EarthquakeTimer = 20;
                    BrainYPosition = 0x00c4;
                    CorpseTileTransferIndex = 0;
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceLoadCorpseTiles;
                    FunctionTimer = 0x0100;
                }
                else
                {
                    BrainYPosition = candidateBrainY;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceLoadCorpseTiles:
                // ProcessSpriteTilesTransfers handles one record per call and notices the
                // following zero terminator on the sixth call, just like the Baby tile list.
                spriteTileTransfer = CreateNextCorpseTileTransfer();
                if (CorpseTileTransferIndex == CorpseTileSources.Length)
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSetupFadeToGrey;
                    FunctionTimer = 0x0020;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSetupFadeToGrey:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    GreyTransitionCounter = 0;
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeToGrey;
                    FunctionTimer = 0;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeToGrey:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    ushort paletteIndex = GreyTransitionCounter;
                    GreyTransitionCounter++;
                    paletteRequested = paletteIndex < 8;
                    if (paletteIndex < 8)
                    {
                        FunctionTimer = 0x0010;
                    }
                    else
                    {
                        SetHeadInstructionList(HeadCorpseInstructionList);
                        Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceCorpseTipsOver;
                        FunctionTimer = 0x0100;
                    }
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceCorpseTipsOver:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceCorpseRotsAway;
                    BrainProperties = BrainProperties.With(EnemyProperties.IgnoreSamusCollision);
                    HitboxesEnabled = false;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceCorpseRotsAway:
                // Retail initialization happens when the brain enemy slot is created, many
                // phases before death. Keep an explicit public initializer for that normal
                // route, but lazily perform the same work for focused debugger fixtures that
                // start at a late phase. Nothing touches this private table/buffer in between,
                // so the delayed call is state-equivalent while preventing a fake host setup.
                if (!_corpseRotting.IsInitialized)
                    _corpseRotting.Initialize(bus);

                MotherBrainCorpseRottingStepResult corpseRotting = _corpseRotting.Step(
                    bus,
                    BrainXPosition,
                    BrainYPosition,
                    randomNumberSeed,
                    mainEnemyExecutionCounter);
                corpseRottingVramTransfers.AddRange(corpseRotting.VramTransfers);
                corpseDustRequests.AddRange(corpseRotting.DustRequests);
                if (corpseRotting.StillRotting)
                    break;

                // Carry clear at `$B1DB` makes the brain invisible/non-solid, clears its
                // second property word, and queues stop + escape music through the ordinary
                // eight-frame-delay music queue. `$14` is loaded and immediately decremented
                // by the fallthrough into `$B211`, leaving observable timer `$13` now.
                BrainProperties = BrainProperties
                    .With(EnemyProperties.Invisible)
                    .Without(EnemyProperties.ProcessInstructions);
                BrainProperties2 = 0;
                musicStopQueued = true;   // Queue music value `$0000`.
                escapeMusicQueued = true; // Queue music value `$FF24`.
                Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequence20FrameDelay;
                FunctionTimer = 0x0014;
                goto case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequence20FrameDelay;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequence20FrameDelay:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    // The decapitated brain remains at the corpse coordinates for the full
                    // delay. `$B216/$B219` finally park it at (0,0) before escape-timer tile
                    // loading begins; this is a real actor-coordinate mutation, not rendering.
                    BrainXPosition = 0;
                    BrainYPosition = 0;
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceLoadEscapeTimerTiles;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceLoadEscapeTimerTiles:
                escapeSequenceTileTransfers.Add(CreateNextEscapeTimerTileTransfer());
                if (EscapeTimerTileTransferIndex == EscapeTimerTileTransfers.Length)
                {
                    // Carry set after entry six clears the shared list cursor, installs
                    // `$B26D`, and falls through far enough to emit exploded-door entry zero
                    // during this same enemy AI call.
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceStartEscape;
                    goto case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceStartEscape;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceStartEscape:
                escapeSequenceTileTransfers.Add(CreateNextExplodedDoorTileTransfer());
                if (ExplodedDoorTileTransferIndex != ExplodedDoorTileTransfers.Length)
                    break;

                // The second door record observes the zero terminator and performs the full
                // `$B275-$B2CD` handoff on that call: copy fourteen colors (palette zero is
                // skipped), start escape music, hold the quake indefinitely, create four
                // palette-FX objects, disable the old unpause hook, and initialize text.
                explodedDoorPaletteRequested = true; // `$A9:9534`, 14 colors -> target `$0122`.
                escapeMusicTrackQueued = true;        // Eight-frame-delay music value `$0007`.
                EarthquakeType = 5;
                EarthquakeTimer = 0xffff;
                escapePaletteFxRequests.AddRange([0xffc9, 0xffcd, 0xffd1, 0xffd5]);
                MotherBrainUnpauseHookEnabled = false;
                escapeTypewriterSetupRequested = true;
                FunctionTimer = 0x0020;
                Phase = alternateEscapeText
                    ? MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSpawnTimeBombSetSubtitle
                    : MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceTypeOutZebesEscapeText;
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSpawnTimeBombSetSubtitle:
                // `$B2D1` decrements first. BPL does *not* merely wait: it branches directly
                // into `$B2E3`, so the underlying typewriter advances during all 32..0
                // subtitle-delay values. On underflow the native code installs `$B2E3`,
                // spawns the persistent Japanese subtitle projectile, and falls through to
                // that same typewriter call. Keep the producer request separate because the
                // bank-$86 allocator owns whether a physical slot is actually available.
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceTypeOutZebesEscapeText;
                    timeBombSetSubtitleSpawnRequested = true;
                }
                goto case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceTypeOutZebesEscapeText;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceTypeOutZebesEscapeText:
                // `$B2E3` calls the independently owned typewriter with list pointer `$2610`.
                // A request plus explicit completion input preserves that scheduler boundary:
                // the boss never invents character cadence, yet its carry-dependent handoff
                // remains directly testable and debuggable.
                typewriterStepRequested = true;
                typewriterTextPointer = 0x2610;
                if (typewriterFinished)
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceDoorExplodingStartTimer;
                    FunctionTimer = 0x0020;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceDoorExplodingStartTimer:
                // `$B2F9` always runs the periodic dust producer before touching the escape
                // countdown. The freshly loaded `$20` therefore receives 33 calls (32..0,
                // then `$FFFF`) and can emit dust even on the transition call.
                escapeDoorExplosion = GenerateEscapeDoorExplosion(nextRandomNumber);
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) == 0)
                    break;

                // Samus command `$0F`, TimerStatus `$0002`, boss bit `$02`, and event `$0E`
                // belong to four different native subsystems. Publish every edge instead of
                // silently writing the wrong debug room's area flags inside this actor.
                timerHandlingEnableRequested = true;
                motherBrainEscapeTimerStartRequested = true;
                motherBrainBossBitRequested = true;
                zebesTimebombEventRequested = true;
                Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceBlowUpEscapeDoor;
                DeathExplosionIntervalTimer = 0;
                EscapeDoorIndex = 0;
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceBlowUpEscapeDoor:
                // `$B3A3` invokes the ordinary highest-free-slot allocator eight times with
                // parameters zero through seven. Allocation success is intentionally decided
                // by the shared enemy-projectile system, not assumed by the boss actor.
                for (ushort parameter = 0; parameter < 8; parameter++)
                    escapeDoorParticleSpawns.Add(new(parameter));

                Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceKeepEarthquakeGoing;
                escapeDoorPlm = new(
                    BlockX: 0x00,
                    BlockY: 0x06,
                    PlmEntry: RoomPlmHeaders.MotherBrainsRoomEscapeDoor);
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceKeepEarthquakeGoing:
                // EarthquakeTimer is global WRAM and is normally decremented outside enemy
                // AI. A supplied sample is therefore authoritative. `$B33C` changes only a
                // visible zero to `$FFFF`; every nonzero value, including `$FFFF`, survives.
                if (globalEarthquakeTimer is ushort observedEarthquakeTimer)
                    EarthquakeTimer = observedEarthquakeTimer;
                if (EarthquakeTimer == 0)
                {
                    EarthquakeTimer = 0xffff;
                    earthquakeTimerRefreshed = true;
                }
                break;

            default:
                throw new InvalidOperationException($"Unsupported rainbow-beam phase {Phase}.");
        }

        return new MotherBrainRainbowBeamAttackStepResult(
            phaseBefore,
            Phase,
            movement,
            healthBefore,
            samus.Health,
            missilesBefore,
            samus.Missiles,
            supersBefore,
            samus.SuperMissiles,
            bombsBefore,
            samus.PowerBombs,
            soundQueued,
            paletteRequested,
            explosion,
            unlockedSamus,
            AngularWidth,
            FunctionTimer,
            EarthquakeType,
            EarthquakeTimer,
            chargeSoundQueued,
            bodyWalkRequested,
            HeadInstructionList,
            _headInstructionListRequestSerial != headInstructionListRequestSerialBefore,
            NeckAngleDelta,
            LowerNeckMovementIndex,
            UpperNeckMovementIndex,
            bodyPostureRequested,
            finishOffAttack,
            spriteTileTransfer,
            babySpawnRequested,
            finalBeamSoundQueued,
            phase3Attack,
            deathExplosions,
            corpseRottingVramTransfers,
            corpseDustRequests,
            musicStopQueued,
            escapeMusicQueued,
            escapeSequenceTileTransfers,
            explodedDoorPaletteRequested,
            escapeMusicTrackQueued,
            escapePaletteFxRequests,
            escapeTypewriterSetupRequested,
            typewriterStepRequested,
            typewriterTextPointer,
            timeBombSetSubtitleSpawnRequested,
            escapeDoorExplosion,
            timerHandlingEnableRequested,
            motherBrainEscapeTimerStartRequested,
            motherBrainBossBitRequested,
            zebesTimebombEventRequested,
            escapeDoorParticleSpawns,
            escapeDoorPlm,
            earthquakeTimerRefreshed);
    }

}
