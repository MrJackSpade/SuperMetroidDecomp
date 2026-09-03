namespace SuperMetroid.Core.Game;

/// <summary>
/// Mother Brain's resurrection and phase-two handoff. These states are physically adjacent
/// in bank $A9 but are kept out of the fake-death/tube file because they own a different
/// actor surface: articulated neck geometry, body bytecode, combat health, and dynamic OBJ
/// tile transfers.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort MotherBrainLegTileTransferList = 0x8f8f;
    private const ushort MotherBrainCrouchedInstruction = 0x9a02;
    private const ushort MotherBrainSlowUncrouchInstruction = 0x99aa;
    private const ushort MotherBrainStretchingHeadInstruction = 0x9b7f;
    private const ushort MotherBrainFromGrayPalettePointerTable = 0xed9c;
    private const ushort MotherBrainNeutralPhaseTwoHeadInstruction = 0x9c87;
    private const ushort MotherBrainFourOnionRingsInstruction = 0x9d3d;
    private const ushort MotherBrainBombPhaseTwoHeadInstruction = 0x9ecc;
    private const ushort MotherBrainLaserHeadInstruction = 0x9f34;

    private Action<ushort>? _setMotherBrainLayerBlendingDefaultConfig;
    private Action<ushort, ushort>? _setMotherBrainBg2Scroll;

    private static readonly ushort[] MotherBrainAscentDustXPositions =
        [0x003d, 0x0054, 0x0020, 0x0035, 0x005a, 0x0043, 0x0067, 0x0029];

    /// <summary>Dispatches $A9:8D49-$8F45 without collapsing native same-frame fallthroughs.</summary>
    private void RunMotherBrainPhaseTwoAscent(
        MotherBrainEnemyState state,
        SamusState? samus,
        byte nmiFrameCounter8,
        SamusBombProjectileSystem? sharedProjectiles = null)
    {
        switch (state.Function)
        {
            case MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Brain:
                SetupMotherBrainPhaseTwoBrain(state, samus, nmiFrameCounter8);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentPauseForSuspense:
                PauseBeforeMotherBrainRises(state, samus, nmiFrameCounter8);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentPrepareForRising:
                PrepareMotherBrainForRising(state, samus, nmiFrameCounter8);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentLoadLegTiles:
                LoadMotherBrainLegTiles(state, samus, nmiFrameCounter8);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentContinuePausing:
                ContinueMotherBrainAscentPause(state);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentStartMusicAndEarthquake:
                StartMotherBrainAscent(state);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentRaiseMotherBrain:
                RaiseMotherBrain(state, nmiFrameCounter8);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentWaitUntilUncrouched:
                WaitForMotherBrainToFinishUncrouching(state);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentTransitionFromGray:
                TransitionMotherBrainFromGray(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseStretchingShakeHead:
                ShakeMotherBrainHeadMenacingly(state, samus, nmiFrameCounter8);
                return;
            case MotherBrainBodyFunction.SecondPhaseStretchingBringHeadUp:
                BringMotherBrainHeadBackUp(state, samus, nmiFrameCounter8);
                return;
            case MotherBrainBodyFunction.SecondPhaseStretchingFinish:
                FinishMotherBrainStretching(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseThinking:
                RunMotherBrainSecondPhaseThinking(state, samus, sharedProjectiles);
                return;
            case MotherBrainBodyFunction.SecondPhaseTryAttack:
                RunMotherBrainSecondPhaseTryAttack(state, samus);
                return;
            case MotherBrainBodyFunction.SecondPhaseBombDecideWalking:
                DecideMotherBrainBombWalking(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseBombWalkingBackwards:
                ContinueMotherBrainBombWalk(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseBombCrouch:
                CrouchMotherBrainForBomb(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseBombFired:
                WaitAfterMotherBrainBomb(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseBombStandUp:
                StandMotherBrainAfterBomb(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseLaserPositionHeadQuickly:
                PositionMotherBrainHeadQuicklyForLaser(
                    state,
                    RequireMotherBrainCombatSamus(samus));
                return;
            case MotherBrainBodyFunction.SecondPhaseLaserPositionHeadSlowlyAndFire:
                PositionMotherBrainHeadSlowlyAndFireLaser(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseLaserFinishAttack:
                FinishMotherBrainLaserAttack(state, samus);
                return;
            case MotherBrainBodyFunction.SecondPhaseHandBeam:
                RunMotherBrainHandBeamAttack(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseRainbowExtendNeck:
            case MotherBrainBodyFunction.SecondPhaseRainbowStartCharging:
            case MotherBrainBodyFunction.SecondPhaseRainbowRetractNeck:
            case MotherBrainBodyFunction.SecondPhaseRainbowWaitForCharge:
            case MotherBrainBodyFunction.SecondPhaseRainbowExtendNeckDown:
            case MotherBrainBodyFunction.SecondPhaseRainbowStartFiring:
            case MotherBrainBodyFunction.SecondPhaseRainbowMoveSamusTowardWall:
            case MotherBrainBodyFunction.SecondPhaseRainbowOneFrameDelay:
            case MotherBrainBodyFunction.SecondPhaseRainbowStartDrainingSamus:
            case MotherBrainBodyFunction.SecondPhaseRainbowDrainingSamus:
            case MotherBrainBodyFunction.SecondPhaseRainbowFinishFiring:
            case MotherBrainBodyFunction.SecondPhaseRainbowLetSamusFall:
            case MotherBrainBodyFunction.SecondPhaseRainbowWaitForSamusToLand:
            case MotherBrainBodyFunction.SecondPhaseRainbowLowerHead:
            case MotherBrainBodyFunction.SecondPhaseRainbowDecideNextAction:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOff:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffStandUp:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffAdmire:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffChargeFinalBeam:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffLoadBabyTiles:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffFireFinalBeam:
            case MotherBrainBodyFunction.SecondPhaseFinalRainbowBeamHolding:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyTakenAback:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyRegainBalance:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyFiringRainbowBeam:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyRainbowBeamRunOut:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyMoveToBackOfRoom:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyGoIntoLowPowerMode:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyPrepareTransitionToGrey:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyTransitionToGrey:
            case MotherBrainBodyFunction.SecondPhaseReviveInanimateGrey:
            case MotherBrainBodyFunction.SecondPhaseReviveShowSignsOfLife:
            case MotherBrainBodyFunction.SecondPhaseReviveTransitionFromGrey:
            case MotherBrainBodyFunction.SecondPhaseReviveWakeUp:
            case MotherBrainBodyFunction.SecondPhaseReviveWakeUpStretch:
            case MotherBrainBodyFunction.SecondPhaseReviveWalkUpToBaby:
            case MotherBrainBodyFunction.SecondPhaseRevivePrepareNeckForBabyDeath:
            case MotherBrainBodyFunction.SecondPhaseReviveFinishPreparingForBabyDeath:
            case MotherBrainBodyFunction.SecondPhaseMurderBabyAttack:
            case MotherBrainBodyFunction.SecondPhaseMurderBabyAttackCooldown:
            case MotherBrainBodyFunction.SecondPhasePrepareForFinalBabyAttack:
            case MotherBrainBodyFunction.SecondPhaseExecuteFinalBabyAttack:
            case MotherBrainBodyFunction.SecondPhaseFinalBabyAttackHolding:
            case MotherBrainBodyFunction.ThirdPhaseRecoverMakeSomeDistance:
            case MotherBrainBodyFunction.ThirdPhaseRecoverSetupForFighting:
            case MotherBrainBodyFunction.ThirdPhaseFightingMain:
            case MotherBrainBodyFunction.ThirdPhaseFightingAttackCooldown:
                RunLiveMotherBrainRainbowBeam(
                    state,
                    RequireMotherBrainCombatSamus(samus),
                    nmiFrameCounter8,
                    sharedProjectiles);
                return;
            default:
                throw new InvalidDataException(
                    $"Mother Brain phase-two function $A9:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private void SetupMotherBrainPhaseTwoBrain(
        MotherBrainEnemyState state,
        SamusState? samus,
        byte nmiFrameCounter8)
    {
        // `$34` is the room's verified Mother Brain phase-two color-math configuration.
        state.LayerBlendingDefaultConfig =
            (ushort)LayerBlendingConfiguration.MotherBrainPhaseTwo;
        RequireSetLayerBlendingDefaultConfig(state.LayerBlendingDefaultConfig);
        state.BrainFunction = MotherBrainBrainFunction.SetupBrainAndNeckToBeDrawn;

        // Both records become tangible before the suspense pause, but remain invisible for
        // their separate native reasons: the body waits for `$8DEC`, while the head's $0100
        // bit deliberately selects the custom articulated draw hook.
        state.Body.Properties =
            state.Body.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        state.Head!.Properties =
            state.Head.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        state.Head.Health = 0x4650;
        state.Function = MotherBrainBodyFunction.FakeDeathAscentPauseForSuspense;
        state.FunctionTimer = 0x0080;

        // `$8D49` falls directly into `$8D79`; the freshly written timer is observed as
        // $007F before another enemy slot can run.
        PauseBeforeMotherBrainRises(state, samus, nmiFrameCounter8);
    }

    private void PauseBeforeMotherBrainRises(
        MotherBrainEnemyState state,
        SamusState? samus,
        byte nmiFrameCounter8)
    {
        if (!DecrementMotherBrainTimerPastZero(state))
            return;

        state.Function = MotherBrainBodyFunction.FakeDeathAscentPrepareForRising;
        state.FunctionTimer = 0x0020;
        PrepareMotherBrainForRising(state, samus, nmiFrameCounter8);
    }

    private void PrepareMotherBrainForRising(
        MotherBrainEnemyState state,
        SamusState? samus,
        byte nmiFrameCounter8)
    {
        if (!DecrementMotherBrainTimerPastZero(state))
            return;

        // Bank $88 installs a fixed per-scanline main-screen table that prevents the newly
        // loaded leg sprites from appearing over the floor. The renderer consumes this
        // typed lifetime rather than pretending it is a body-visibility flag.
        state.RisingHdmaActive = true;
        state.Head!.Properties = state.Head.Properties.With(EnemyProperties.Invisible);
        SetMotherBrainInstructionList(state.Head, MotherBrainInitialHeadInstruction);
        state.Function = MotherBrainBodyFunction.FakeDeathAscentLoadLegTiles;
        state.FunctionTimer = 0x0100;

        // The first $0200-byte transfer is queued on the same frame as HDMA creation.
        LoadMotherBrainLegTiles(state, samus, nmiFrameCounter8);
    }

    private void LoadMotherBrainLegTiles(
        MotherBrainEnemyState state,
        SamusState? samus,
        byte nmiFrameCounter8)
    {
        if (!ProcessMotherBrainSpriteTileTransfer(
                state,
                MotherBrainLegTileTransferList))
        {
            return;
        }

        state.Function = MotherBrainBodyFunction.FakeDeathAscentContinuePausing;

        // `$8DBD` has no return between the state write and `$8DC3`; the still-$0100 timer
        // loses its first tick on the final DMA frame.
        ContinueMotherBrainAscentPause(state);
    }

    private bool ProcessMotherBrainSpriteTileTransfer(
        MotherBrainEnemyState state,
        ushort firstEntryPointer)
    {
        ushort entry = state.SpriteTileTransferEntryPointer == 0
            ? firstEntryPointer
            : state.SpriteTileTransferEntryPointer;
        int address = 0xa90000 | entry;
        ushort byteCount = ReadWord(_bus!, address);
        if (byteCount == 0)
        {
            state.SpriteTileTransferEntryPointer = 0;
            return true;
        }

        int source = _bus!.ReadByte(address + 2) |
            (_bus.ReadByte(address + 3) << 8) |
            (_bus.ReadByte(address + 4) << 16);
        ushort destination = ReadWord(_bus, address + 5);
        var bytes = new byte[byteCount];
        for (int index = 0; index < bytes.Length; index++)
            bytes[index] = _bus.ReadByte(source + index);
        _vram!.LoadBytes(destination * 2, bytes);

        ushort next = unchecked((ushort)(entry + 7));
        if (ReadWord(_bus, 0xa90000 | next) == 0)
        {
            state.SpriteTileTransferEntryPointer = 0;
            return true;
        }

        state.SpriteTileTransferEntryPointer = next;
        return false;
    }

    private void ContinueMotherBrainAscentPause(MotherBrainEnemyState state)
    {
        if (!DecrementMotherBrainTimerPastZero(state))
            return;

        state.Body.XPosition = 0x003b;
        state.Body.YPosition = 0x0117;
        PublishMotherBrainBg2Scroll(state, 0xffe5, 0xff27);
        state.HitboxesEnabled = 7;
        state.Function = MotherBrainBodyFunction.FakeDeathAscentStartMusicAndEarthquake;
    }

    private void StartMotherBrainAscent(MotherBrainEnemyState state)
    {
        SetMotherBrainInstructionList(state.Body, MotherBrainCrouchedInstruction);
        state.Head!.InstructionTimer = 1;
        state.Body.Properties = state.Body.Properties.Without(EnemyProperties.Invisible);
        state.Body.XPosition = 0x003b;
        state.Body.YPosition = 0x0117;
        PublishMotherBrainBg2Scroll(state, 0xffe5, 0xff27);
        state.RequestMusic(rawTrack: 5, delayFrames: 8);
        EarthquakeType = 2;
        EarthquakeTimer = 0x0100;
        state.NeckAngleDelta = 0x0050;
        state.NeckMovementEnabled = true;
        state.LowerNeckMovementIndex = 8;
        state.UpperNeckMovementIndex = 6;
        state.Function = MotherBrainBodyFunction.FakeDeathAscentRaiseMotherBrain;
    }

    private void RaiseMotherBrain(MotherBrainEnemyState state, byte nmiFrameCounter8)
    {
        if ((nmiFrameCounter8 & 3) != 0)
            return;

        SpawnMotherBrainAscentDust(state);
        state.Bg2YScroll = unchecked((ushort)(state.Bg2YScroll + 2));
        state.Body.YPosition = unchecked((ushort)(state.Body.YPosition - 2));
        PublishMotherBrainBg2Scroll(state, state.Bg2XScroll, state.Bg2YScroll);
        if (state.Body.YPosition >= 0x00bd)
            return;

        state.EnemyBg2TilemapSize = 0x0140;
        state.EnemyBg2TilemapTransferRequested = true;
        state.Body.YPosition = 0x00bc;
        EarthquakeTimer = 0;
        state.RisingHdmaActive = false;
        SetMotherBrainInstructionList(state.Body, MotherBrainSlowUncrouchInstruction);
        state.Function = MotherBrainBodyFunction.FakeDeathAscentWaitUntilUncrouched;
    }

    private void SpawnMotherBrainAscentDust(MotherBrainEnemyState state)
    {
        state.BodySubFunctionTimer = unchecked((ushort)(state.BodySubFunctionTimer - 1));
        if (unchecked((short)state.BodySubFunctionTimer) < 0)
            state.BodySubFunctionTimer = 7;

        ushort x = MotherBrainAscentDustXPositions[state.BodySubFunctionTimer];
        ushort random = RequireRandomNumber();
        ushort animation = (random & 0x0100) == 0 ? (ushort)9 : (ushort)0x12;
        SpawnRoomGraphicsDustExplosion(x, 0x00d4, animation);
        state.LastSoundEffect = 0x0029;
    }

    private static void WaitForMotherBrainToFinishUncrouching(MotherBrainEnemyState state)
    {
        if (state.Pose != MotherBrainBodyPose.Standing)
            return;

        state.GrayTransitionCounter = 0;
        state.Function = MotherBrainBodyFunction.FakeDeathAscentTransitionFromGray;
        state.FunctionTimer = 0;
    }

    private void TransitionMotherBrainFromGray(MotherBrainEnemyState state)
    {
        if (!DecrementMotherBrainTimerPastZero(state))
            return;

        state.FunctionTimer = 4;
        ushort paletteStep = state.GrayTransitionCounter++;
        ushort source = ReadWord(
            _bus!,
            0xad0000 | unchecked((ushort)(MotherBrainFromGrayPalettePointerTable + paletteStep * 2)));
        if (source != 0)
        {
            // Fake-death restoration changes only brain sprite colors one through three.
            _cgram!.LoadFromBus(
                _bus!,
                0xad0000 | source,
                colorCount: 3,
                destinationIndex: 0x0122 / 2);
            return;
        }

        state.BrainPaletteHandlingEnabled = true;
        state.Form = 2;
        state.DroolGenerationEnabled = true;
        state.LowerNeckMovementIndex = 6;
        state.UpperNeckMovementIndex = 6;
        state.NeckAngleDelta = 0x0500;
        state.Function = MotherBrainBodyFunction.SecondPhaseStretchingShakeHead;
        state.FunctionTimer = 0x0017;
    }

    private static void ShakeMotherBrainHeadMenacingly(
        MotherBrainEnemyState state,
        SamusState? samus,
        byte nmiFrameCounter8)
    {
        if (!DecrementMotherBrainTimerPastZero(state))
            return;

        SetMotherBrainInstructionList(state.Head!, MotherBrainStretchingHeadInstruction);
        state.Function = MotherBrainBodyFunction.SecondPhaseStretchingBringHeadUp;
        state.NeckAngleDelta = 0x0040;
        state.FunctionTimer = 0x0100;
        BringMotherBrainHeadBackUp(state, samus, nmiFrameCounter8);
    }

    private static void BringMotherBrainHeadBackUp(
        MotherBrainEnemyState state,
        SamusState? samus,
        byte nmiFrameCounter8)
    {
        if (!DecrementMotherBrainTimerPastZero(state))
            return;

        state.LowerNeckMovementIndex = 2;
        state.UpperNeckMovementIndex = 4;
        state.Function = MotherBrainBodyFunction.SecondPhaseStretchingFinish;
        state.FunctionTimer = 0x0040;
        FinishMotherBrainStretching(state);
    }

    private static void FinishMotherBrainStretching(MotherBrainEnemyState state)
    {
        if (!DecrementMotherBrainTimerPastZero(state))
            return;

        state.SmallPurpleBreathGenerationEnabled = true;
        state.Function = MotherBrainBodyFunction.SecondPhaseThinking;
    }

    private void RunMotherBrainSecondPhaseThinking(
        MotherBrainEnemyState state,
        SamusState? samus,
        SamusBombProjectileSystem? sharedProjectiles = null)
    {
        if (state.Head!.Health == 0)
        {
            StartLiveMotherBrainRainbowBeam(
                state,
                RequireMotherBrainCombatSamus(samus),
                sharedProjectiles);
            return;
        }
        if (state.Pose != MotherBrainBodyPose.Standing)
            return;

        ushort random = RequireRandomNumber();
        if (state.Head.Health >= 0x1194)
        {
            if (random < 0x1000)
            {
                // `$B605` installs the attack dispatcher and returns. It does not execute
                // phase zero until the body's next enemy-AI call.
                state.Function = MotherBrainBodyFunction.SecondPhaseTryAttack;
                return;
            }
            HandleMotherBrainWalking(state);
            return;
        }

        if (random < 0x2000)
        {
            HandleMotherBrainWalking(state);
            return;
        }
        if (random >= 0xa000)
        {
            state.Function = MotherBrainBodyFunction.SecondPhaseTryAttack;
            return;
        }
        // `$B644` installs the ordinary red hand-beam dispatcher. It does not execute phase
        // zero until the next body AI call; `$B8F1` belongs to the separate rainbow beam
        // entered only when health reaches zero.
        state.Function = MotherBrainBodyFunction.SecondPhaseHandBeam;
    }

    /// <summary>
    /// Ports the three-entry body dispatcher at <c>$A9:B64B-B780</c>. The selected head
    /// list is ordinary enemy bytecode, so attacks remain synchronized with native map
    /// durations instead of being represented by host timers.
    /// </summary>
    private void RunMotherBrainSecondPhaseTryAttack(
        MotherBrainEnemyState state,
        SamusState? samus)
    {
        switch (state.AttackPhase)
        {
            case MotherBrainAttackPhase.ChooseAttack:
                ChooseMotherBrainSecondPhaseAttack(state, RequireMotherBrainCombatSamus(samus));
                return;

            case MotherBrainAttackPhase.Cooldown:
                // `$B764` decrements before testing. The phase-zero initializer always
                // writes $40, but unchecked arithmetic retains the native zero-underflow
                // behavior if a debugger edits this word while stopped here.
                state.AttackCooldown = unchecked((ushort)(state.AttackCooldown - 1));
                if (state.AttackCooldown == 0)
                    state.AttackPhase = MotherBrainAttackPhase.EndAttack;
                return;

            case MotherBrainAttackPhase.EndAttack:
                state.AttackPhase = MotherBrainAttackPhase.ChooseAttack;
                state.Function = MotherBrainBodyFunction.SecondPhaseThinking;
                return;

            default:
                throw new InvalidDataException(
                    $"Mother Brain attack phase {(ushort)state.AttackPhase} is outside the " +
                    "three-entry cartridge dispatcher.");
        }
    }

    /// <summary>Ports <c>$A9:B65A-B72B</c>, including its unusual stack-return strategy.</summary>
    private void ChooseMotherBrainSecondPhaseAttack(
        MotherBrainEnemyState state,
        SamusState samus)
    {
        state.AttackCooldown = 0x0040;
        state.AttackPhase = MotherBrainAttackPhase.Cooldown;

        byte randomLow = unchecked((byte)RequireRandomNumber());
        SamusMovementType movementType = samus.ReadMovementType(_bus!);
        if ((byte)movementType >= 0x1c)
        {
            throw new InvalidDataException(
                $"Mother Brain attack strategy cannot index Samus movement type ${(byte)movementType:X2}.");
        }

        if (IsMotherBrainAirAttackMovement(movementType))
        {
            // The native helper removes its caller's return address: every airborne route
            // commits immediately and skips the proximity/fallback thresholds entirely.
            if (randomLow >= 0x80)
            {
                SetMotherBrainInstructionList(
                    state.Head!,
                    MotherBrainFourOnionRingsInstruction);
                return;
            }

            // `$B702` discards the strategy helper's return address, then installs and
            // jumps to `$B80E`. Calling the first body state here reproduces that same-frame
            // tail call rather than introducing an otherwise visible idle frame.
            state.Function = MotherBrainBodyFunction.SecondPhaseLaserPositionHeadQuickly;
            PositionMotherBrainHeadQuicklyForLaser(state, samus);
            return;
        }

        // Grounded movement types attempt a bomb on the upper half of the current random
        // byte. An already-live bomb falls back to the normal threshold table.
        if (randomLow >= 0x80 && state.BombCounter < 1)
        {
            state.Function = MotherBrainBodyFunction.SecondPhaseBombDecideWalking;
            DecideMotherBrainBombWalking(state);
            return;
        }

        ushort verticalDistance = WrappedMagnitude(unchecked((ushort)(
            state.Head!.YPosition + 4 - samus.YPosition)));
        ReadOnlySpan<byte> thresholds = verticalDistance < 0x20
            ? [0x10, 0x20, 0xd0]
            : [0x40, 0x80, 0xc0];
        int choice = randomLow < thresholds[0]
            ? 0
            : randomLow < thresholds[1]
                ? 1
                : randomLow < thresholds[2]
                    ? 2
                    : 3;

        switch (choice)
        {
            case 0:
                SetMotherBrainInstructionList(
                    state.Head,
                    MotherBrainNeutralPhaseTwoHeadInstruction);
                return;
            case 1:
                SetMotherBrainInstructionList(
                    state.Head,
                    MotherBrainFourOnionRingsInstruction);
                return;
            case 2:
                state.Function = MotherBrainBodyFunction.SecondPhaseLaserPositionHeadQuickly;
                PositionMotherBrainHeadQuicklyForLaser(state, samus);
                return;
            case 3 when state.BombCounter >= 1:
                // `$B6B9` returns without changing the current head list when one bomb is
                // already alive. The body still spends the full cooldown in phase one.
                return;
            default:
                state.Function = MotherBrainBodyFunction.SecondPhaseBombDecideWalking;
                DecideMotherBrainBombWalking(state);
                return;
        }
    }

    private static bool IsMotherBrainAirAttackMovement(SamusMovementType movementType) =>
        movementType is
            SamusMovementType.NormalJumping or
            SamusMovementType.SpinJumping or
            SamusMovementType.Falling or
            SamusMovementType.MorphBallFalling or
            SamusMovementType.UnusedGlitchBallAlternate or
            SamusMovementType.Unused0B or
            SamusMovementType.Unused0C or
            SamusMovementType.Unused0D or
            SamusMovementType.SpringBallInAir or
            SamusMovementType.SpringBallFalling or
            SamusMovementType.WallJumping;

    /// <summary>Ports <c>$A9:B781-B7AB</c>, including its immediate posture decision.</summary>
    private void DecideMotherBrainBombWalking(MotherBrainEnemyState state)
    {
        ushort random = RequireRandomNumber();
        if (random < 0xff80)
        {
            ushort targetX = random >= 0x6000 ? (ushort)0x0040 : (ushort)0x0060;
            ushort targetMinusBody = unchecked((ushort)(targetX - state.Body.XPosition));
            if ((targetMinusBody & 0x8000) != 0)
            {
                state.BodyTargetXPosition = targetX;
                if (!TryWalkMotherBrainBackwards(state, targetX))
                {
                    state.Function = MotherBrainBodyFunction.SecondPhaseBombWalkingBackwards;
                    return;
                }
            }
        }

        DecideWhetherMotherBrainCrouchesForBomb(state);
    }

    /// <summary>Ports stored body state <c>$A9:B7AC</c>.</summary>
    private void ContinueMotherBrainBombWalk(MotherBrainEnemyState state)
    {
        if (TryWalkMotherBrainBackwards(state, state.BodyTargetXPosition))
            DecideWhetherMotherBrainCrouchesForBomb(state);
    }

    /// <summary>
    /// Ports the fallthrough at <c>$A9:B7B7</c>. This routine advances the global RNG—unlike
    /// attack selection—and fires immediately on the lower half; the upper half enters the
    /// ordinary slow-crouch body list before firing.
    /// </summary>
    private void DecideWhetherMotherBrainCrouchesForBomb(MotherBrainEnemyState state)
    {
        if (_nextRandom!() < 0x8000)
        {
            FireMotherBrainBomb(state);
            return;
        }

        state.Function = MotherBrainBodyFunction.SecondPhaseBombCrouch;
        CrouchMotherBrainForBomb(state);
    }

    /// <summary>Ports stored body state <c>$A9:B7C6</c>.</summary>
    private static void CrouchMotherBrainForBomb(MotherBrainEnemyState state)
    {
        if (TryMakeMotherBrainCrouch(state))
            FireMotherBrainBomb(state);
    }

    /// <summary>Ports the unstored firing tail at <c>$A9:B7CB</c>.</summary>
    private static void FireMotherBrainBomb(MotherBrainEnemyState state)
    {
        SetMotherBrainInstructionList(state.Head!, MotherBrainBombPhaseTwoHeadInstruction);
        state.Function = MotherBrainBodyFunction.SecondPhaseBombFired;
        state.FunctionTimer = 0x002c;
    }

    /// <summary>Ports stored body state <c>$A9:B7E8</c> and its stand-up fallthrough.</summary>
    private static void WaitAfterMotherBrainBomb(MotherBrainEnemyState state)
    {
        state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
        if ((state.FunctionTimer & 0x8000) == 0)
            return;

        if (TryMakeMotherBrainStand(state))
        {
            FinishMotherBrainBombAttack(state);
            return;
        }

        state.Function = MotherBrainBodyFunction.SecondPhaseBombStandUp;

        // `$B7F2` falls directly into `$B7F8`, so a crouched body calls the helper twice on
        // this frame. Reinstalling the same list/timer is observable to a debugger and must
        // not be collapsed into a host-only transition delay.
        StandMotherBrainAfterBomb(state);
    }

    /// <summary>Ports stored body state <c>$A9:B7F8</c>.</summary>
    private static void StandMotherBrainAfterBomb(MotherBrainEnemyState state)
    {
        if (TryMakeMotherBrainStand(state))
            FinishMotherBrainBombAttack(state);
    }

    private static void FinishMotherBrainBombAttack(MotherBrainEnemyState state) =>
        state.Function = MotherBrainBodyFunction.SecondPhaseThinking;

    /// <summary>Literal target/pose/list behavior of <c>$A9:C647</c> at delay-table offset six.</summary>
    private static bool TryWalkMotherBrainBackwards(
        MotherBrainEnemyState state,
        ushort targetX)
    {
        ushort targetMinusBody = unchecked((ushort)(targetX - state.Body.XPosition));
        if ((targetMinusBody & 0x8000) == 0)
            return true;
        if (state.Pose != MotherBrainBodyPose.Standing)
            return false;
        if (unchecked((short)(state.Body.XPosition - 0x0030)) < 0)
            return true;

        SetMotherBrainInstructionList(state.Body, 0x9900); // Backwards, medium.
        return false;
    }

    /// <summary>Literal pose/list behavior of <c>$A9:C68E</c>.</summary>
    private static bool TryMakeMotherBrainCrouch(MotherBrainEnemyState state)
    {
        if (state.Pose == MotherBrainBodyPose.Crouched)
            return true;
        if (state.Pose == MotherBrainBodyPose.Standing)
            SetMotherBrainInstructionList(state.Body, MotherBrainCrouchedInstruction);
        return false;
    }

    /// <summary>Literal pose/list behavior of <c>$A9:C670</c>.</summary>
    private static bool TryMakeMotherBrainStand(MotherBrainEnemyState state)
    {
        switch (state.Pose)
        {
            case MotherBrainBodyPose.Standing:
                return true;
            case MotherBrainBodyPose.Crouched:
                SetMotherBrainInstructionList(state.Body, 0x99c6);
                return false;
            case MotherBrainBodyPose.LeaningDown:
                SetMotherBrainInstructionList(state.Body, 0x99e2);
                return false;
            default:
                return false;
        }
    }

    private static SamusState RequireMotherBrainCombatSamus(SamusState? samus) =>
        samus ?? throw new InvalidOperationException(
            "Mother Brain phase-two attack selection requires the active Samus actor.");

    /// <summary>
    /// Ports body state <c>$A9:B80E</c>. Both neck joints initially turn by two angle units
    /// per frame toward one of two authored target indices chosen from the head/Samus Y
    /// ordering; the head instruction list itself is deliberately left untouched here.
    /// </summary>
    private static void PositionMotherBrainHeadQuicklyForLaser(
        MotherBrainEnemyState state,
        SamusState samus)
    {
        RoomEnemySlot head = state.Head!;

        // CMP/BPL tests the sign bit of the wrapped 16-bit subtraction. Normal arena
        // coordinates cannot overflow signed range, but expressing the native flag test
        // explicitly keeps debugger-edited/wrapped coordinates cartridge-faithful too.
        ushort headMinusSamus = unchecked((ushort)(head.YPosition - samus.YPosition));
        ushort targetIndex = (headMinusSamus & 0x8000) == 0 ? (ushort)8 : (ushort)6;
        state.LowerNeckMovementIndex = targetIndex;
        state.UpperNeckMovementIndex = targetIndex;
        state.NeckAngleDelta = 0x0200;
        state.Function = MotherBrainBodyFunction.SecondPhaseLaserPositionHeadSlowlyAndFire;
        state.FunctionTimer = 0x0004;
    }

    /// <summary>
    /// Ports <c>$A9:B839</c>. Its unsigned timer expires only after decrementing through
    /// zero to <c>$FFFF</c>, at which point neck rotation slows and ordinary head bytecode
    /// owns the muzzle animation and projectile spawn.
    /// </summary>
    private static void PositionMotherBrainHeadSlowlyAndFireLaser(
        MotherBrainEnemyState state)
    {
        if (!DecrementMotherBrainTimerPastZero(state))
            return;

        state.NeckAngleDelta = (state.NeckAngleDelta & 0x8000) == 0
            ? (ushort)0x0100
            : (ushort)0xff00;
        SetMotherBrainInstructionList(state.Head!, MotherBrainLaserHeadInstruction);
        state.Function = MotherBrainBodyFunction.SecondPhaseLaserFinishAttack;
        state.FunctionTimer = 0x0010;
    }

    /// <summary>
    /// Ports <c>$A9:B863</c>, including its same-frame jump back into thinking. The attack
    /// phase/cooldown words intentionally remain as selected: the laser body state replaces
    /// `$B64B`, so the native chain does not pass through that dispatcher's end phase.
    /// </summary>
    private void FinishMotherBrainLaserAttack(
        MotherBrainEnemyState state,
        SamusState? samus)
    {
        if (!DecrementMotherBrainTimerPastZero(state))
            return;

        state.LowerNeckMovementIndex = 4;
        state.UpperNeckMovementIndex = 4;
        state.Function = MotherBrainBodyFunction.SecondPhaseThinking;
        RunMotherBrainSecondPhaseThinking(state, samus);
    }

    private static void HandleMotherBrainWalking(MotherBrainEnemyState state)
    {
        // `$A9:C6B8` alters only the walk accumulator and installs a ROM-authored body list;
        // actual displacement remains bytecode-owned, which also keeps BG2 scroll aligned.
        if (state.Pose != MotherBrainBodyPose.Standing)
            return;
        if (state.WalkCounter == 0)
        {
            state.WalkCounter = 1;
            if (state.Body.XPosition >= 0x0030)
                SetMotherBrainInstructionList(state.Body, 0x983c); // Backwards, fast.
            else
                SetMotherBrainInstructionList(state.Body, 0x97a4); // Forwards, medium.
            return;
        }

        state.WalkCounter = unchecked((ushort)(state.WalkCounter + 6));
        if (state.WalkCounter >= 0x0100)
        {
            state.WalkCounter = 0x0080;
            if (state.Body.XPosition < 0x0080)
                SetMotherBrainInstructionList(state.Body, 0x97a4);
        }
        else if (state.Body.XPosition < 0x0030)
        {
            SetMotherBrainInstructionList(state.Body, 0x97a4);
        }
    }

    /// <summary>
    /// Literal body/BG2 movement helper at <c>$A9:9579</c>. The signed Y operand is added
    /// to the actor and subtracted from the background, keeping the large BG2 body art
    /// rigidly attached to its small OBJ hitbox record. X movement happens before this call
    /// in the cartridge, so the derived scroll must sample the already-updated body X.
    /// </summary>
    private void MoveMotherBrainBody(short xDisplacement, short yDisplacement)
    {
        MotherBrainEnemyState state = _motherBrain ?? throw new InvalidOperationException(
            "Mother Brain body bytecode ran without its multipart encounter state.");
        state.Body.XPosition = unchecked((ushort)(state.Body.XPosition + xDisplacement));
        state.Body.YPosition = unchecked((ushort)(state.Body.YPosition + yDisplacement));
        PublishMotherBrainBg2Scroll(
            state,
            unchecked((ushort)(0x0022 - state.Body.XPosition)),
            unchecked((ushort)(state.Bg2YScroll - yDisplacement)));
    }

    /// <summary>
    /// Posture-transition variant at <c>$A9:9552</c>. Its X register is a scroll bias, not
    /// actor displacement; only Y changes the physical body coordinate.
    /// </summary>
    private void MoveMotherBrainBodyWithScrollBias(
        short yDisplacement,
        short horizontalScrollBias)
    {
        MotherBrainEnemyState state = _motherBrain ?? throw new InvalidOperationException(
            "Mother Brain posture bytecode ran without its multipart encounter state.");
        state.Body.YPosition = unchecked((ushort)(state.Body.YPosition + yDisplacement));
        PublishMotherBrainBg2Scroll(
            state,
            unchecked((ushort)(0x0022 + horizontalScrollBias - state.Body.XPosition)),
            unchecked((ushort)(state.Bg2YScroll - yDisplacement)));
    }

    /// <summary>Shared four-frame earthquake produced by each planted walking frame.</summary>
    private void RunMotherBrainFootstep()
    {
        EarthquakeType = 1;
        EarthquakeTimer = 4;

        // Form three additionally queues library-three sound $16. Phase two normally never
        // reaches that branch, but retaining it here makes the shared gait opcode complete.
        if (_motherBrain?.Form == 3)
            _motherBrain.LastSoundEffectLibrary3 = 0x0016;
    }

    private static void SetMotherBrainInstructionList(RoomEnemySlot slot, ushort pointer)
    {
        slot.CurrentInstruction = pointer;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private void PublishMotherBrainBg2Scroll(
        MotherBrainEnemyState state,
        ushort x,
        ushort y)
    {
        state.Bg2XScroll = x;
        state.Bg2YScroll = y;
        state.HasBg2ScrollOverride = true;
        RequireSetMotherBrainBg2Scroll(x, y);
    }
}
