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

    private Action<ushort>? _setMotherBrainLayerBlendingDefaultConfig;
    private Action<ushort, ushort>? _setMotherBrainBg2Scroll;

    private static readonly ushort[] MotherBrainAscentDustXPositions =
        [0x003d, 0x0054, 0x0020, 0x0035, 0x005a, 0x0043, 0x0067, 0x0029];

    /// <summary>Dispatches $A9:8D49-$8F45 without collapsing native same-frame fallthroughs.</summary>
    private void RunMotherBrainPhaseTwoAscent(
        MotherBrainEnemyState state,
        SamusState? samus,
        byte nmiFrameCounter8)
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
                RunMotherBrainSecondPhaseThinking(state);
                return;
            default:
                throw new NotSupportedException(
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
        _setMotherBrainLayerBlendingDefaultConfig?.Invoke(state.LayerBlendingDefaultConfig);
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
        ushort random = _readRandomNumber?.Invoke() ?? 0;
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

    private void ShakeMotherBrainHeadMenacingly(
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

    private void BringMotherBrainHeadBackUp(
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

    private void RunMotherBrainSecondPhaseThinking(MotherBrainEnemyState state)
    {
        if (state.Head!.Health == 0)
        {
            throw new NotSupportedException(
                "Mother Brain phase-two death/rainbow-beam transition $A9:BB3B is not translated yet.");
        }
        if (state.Pose != MotherBrainBodyPose.Standing)
            return;

        ushort random = _readRandomNumber?.Invoke() ?? 0;
        if (state.Head.Health >= 0x1194)
        {
            if (random < 0x1000)
            {
                throw new NotSupportedException(
                    "Mother Brain phase-two attack decision $A9:B64B is not translated yet.");
            }
            HandleMotherBrainWalking(state);
            return;
        }

        if (random < 0x2000)
        {
            HandleMotherBrainWalking(state);
            return;
        }
        throw new NotSupportedException(
            random >= 0xa000
                ? "Mother Brain low-health attack decision $A9:B64B is not translated yet."
                : "Mother Brain death-beam attack $A9:B8F1 is not translated yet.");
    }

    private static void HandleMotherBrainWalking(MotherBrainEnemyState state)
    {
        // `$A9:C6B8` alters only the walk accumulator and installs a ROM-authored body list;
        // actual displacement remains bytecode-owned, which also keeps BG2 scroll aligned.
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
        _setMotherBrainBg2Scroll?.Invoke(x, y);
    }
}
