using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Retail dead-sidehopper actors used by the Shitroid encounter and Tourian corpse rooms.
/// </summary>
public sealed partial class RoomEnemySystem
{
    public const ushort DeadSidehopperDefinition = 0xed7f;

    private const ushort DeadSidehopperInitialInstruction = 0xece3;
    private const ushort DeadSidehopperLandingInstruction = 0xecac;
    private const ushort DeadSidehopperCorpseInstruction = 0xece9;
    private const ushort DeadSidehopperAlternateInstruction = 0xecef;
    private const ushort DeadSidehopperConfiguration0 = 0xdd68;
    private const ushort DeadSidehopperConfiguration2 = 0xdd78;
    private const ushort DeadSidehopperCopyFunction0 = 0xe4f5;
    private const ushort DeadSidehopperMoveFunction0 = 0xe468;
    private const ushort DeadSidehopperGraphicsInitFunction0 = 0xdec1;
    private const ushort DeadSidehopperCopyFunction2 = 0xe5f6;
    private const ushort DeadSidehopperMoveFunction2 = 0xe564;
    private const ushort DeadSidehopperGraphicsInitFunction2 = 0xdf08;
    private const ushort DeadMonsterFinishedFunction = 0xdc08;
    private const int DeadMonsterWorkBufferAddress = 0x7e2000;
    private const int DeadMonsterTileDataAddress = 0xb7c000;
    private const ushort DeadMonsterSolidProperty = 0x8000;
    private const ushort DeadMonsterInteractionRejectedProperty = 0x0400;

    private static readonly DeadSidehopperGraphicsCopy[][] DeadSidehopperInitialGraphicsCopies =
    [
        [
            new(0x0040, 0x0040, 0x0060),
            new(0x0200, 0x00a0, 0x00a0),
            new(0x0400, 0x0140, 0x00a0),
            new(0x0600, 0x01e0, 0x00a0),
            new(0x0800, 0x0280, 0x00a0),
        ],
        [
            new(0x0120, 0x0320, 0x0040),
            new(0x0320, 0x03c0, 0x00a0),
            new(0x0520, 0x0460, 0x00a0),
            new(0x0720, 0x0500, 0x00a0),
            new(0x0920, 0x05a0, 0x00a0),
        ],
    ];

    // Each entry names the first planes-0/1 word of one eight-pixel-wide tile column.
    // Planes 2/3 live eight words later. Variant zero's upper two columns do not begin
    // until row eight; variant two has the inverse three-column lower silhouette.
    private static ReadOnlySpan<ushort> DeadSidehopperColumnWordOffsets0 =>
        [0, 16, 32, 48, 64];

    private static ReadOnlySpan<ushort> DeadSidehopperColumnMinimumY0 =>
        [8, 8, 0, 0, 0];

    private static ReadOnlySpan<ushort> DeadSidehopperColumnWordOffsets2 =>
        [400, 416, 432, 448, 464];

    private static ReadOnlySpan<ushort> DeadSidehopperColumnMinimumY2 =>
        [0, 0, 8, 8, 8];

    private readonly DeadSidehopperEnemyState?[] _deadSidehopperStates =
        new DeadSidehopperEnemyState?[MaximumEnemyCount];
    private readonly List<VramWriteEntry> _deadSidehopperFrameVramTransfers = [];
    private RoomLevelData? _deadSidehopperLevel;
    private ushort _deadSidehopperCameraX;

    /// <summary>Companion/corpse state indexed by physical enemy slot.</summary>
    public IReadOnlyList<DeadSidehopperEnemyState?> DeadSidehoppers =>
        _deadSidehopperStates;

    /// <summary>VRAM writes emitted by every rotting sidehopper during the latest frame.</summary>
    public IReadOnlyList<VramWriteEntry> LastDeadSidehopperVramTransfers =>
        _deadSidehopperFrameVramTransfers;

    /// <summary>Last library-two dust sound requested by a completed sidehopper row.</summary>
    public ushort? LastDeadSidehopperSoundEffect { get; private set; }

    private void ResetDeadSidehopperRoomState()
    {
        Array.Clear(_deadSidehopperStates);
        _deadSidehopperFrameVramTransfers.Clear();
        LastDeadSidehopperSoundEffect = null;
        _deadSidehopperLevel = null;
        _deadSidehopperCameraX = 0;
    }

    private void BeginDeadSidehopperFrame()
    {
        _deadSidehopperFrameVramTransfers.Clear();
        LastDeadSidehopperSoundEffect = null;
    }

    /// <summary>Ports <c>DeadSidehopper_Init</c> at <c>$A9:D7B6</c>.</summary>
    private void InitializeDeadSidehopper(RoomEnemySlot slot)
    {
        if (_deadSidehopperStates[slot.SlotIndex] is not null)
            throw new InvalidDataException($"Dead sidehopper slot {slot.SlotIndex} was initialized twice.");

        switch (slot.Parameter1)
        {
            case 0:
                InitializeShitroidVictimSidehopper(slot);
                break;
            case 2:
                InitializeAlternateDeadSidehopper(slot);
                break;
            default:
                throw new InvalidDataException(
                    $"Dead sidehopper parameter 1 ${slot.Parameter1:X4} is not authored by retail AI.");
        }
    }

    /// <summary>Ports the Shitroid victim layout at <c>$A9:D7C4</c>.</summary>
    private void InitializeShitroidVictimSidehopper(RoomEnemySlot slot)
    {
        // The native mask clears the solid bit and any inherited process-offscreen bit,
        // then explicitly reinstalls only process-offscreen. Do not replace this with an
        // enum union: preserving the unrelated population-property bits is observable.
        slot.Properties = slot.Properties.Replace(
            EnemyProperties.SolidToSamus | EnemyProperties.ProcessOffScreen,
            EnemyProperties.ProcessOffScreen);
        if (_slots[0].Properties.HasAny(EnemyProperties.Invisible))
            slot.Properties = slot.Properties.With(EnemyProperties.Deleted);

        slot.XPosition = 488;
        slot.YPosition = 184;
        slot.PaletteIndex = 0x0200;
        slot.YRadius = 21;
        SetDeadSidehopperInstruction(slot, DeadSidehopperInitialInstruction);

        DeadSidehopperEnemyState state = InitializeDeadSidehopperCorpseState(
            slot,
            graphicsVariant: 0,
            DeadSidehopperConfiguration0,
            DeadSidehopperCopyFunction0,
            DeadSidehopperMoveFunction0,
            DeadSidehopperGraphicsInitFunction0);
        state.Function = DeadSidehopperAiFunction.AliveWaitForCamera;
        state.PaletteStage = 0;
        state.HorizontalVelocity = 96;
        state.VerticalVelocity = 256;
    }

    /// <summary>Ports the already-dead Tourian layout at <c>$A9:D825</c>.</summary>
    private void InitializeAlternateDeadSidehopper(RoomEnemySlot slot)
    {
        slot.PaletteIndex = 0x0e00;
        SetDeadSidehopperInstruction(slot, DeadSidehopperAlternateInstruction);

        DeadSidehopperEnemyState state = InitializeDeadSidehopperCorpseState(
            slot,
            graphicsVariant: 2,
            DeadSidehopperConfiguration2,
            DeadSidehopperCopyFunction2,
            DeadSidehopperMoveFunction2,
            DeadSidehopperGraphicsInitFunction2);
        state.PaletteStage = 0xffff;
        state.Function = DeadSidehopperAiFunction.WaitForSamusCollision;
    }

    private DeadSidehopperEnemyState InitializeDeadSidehopperCorpseState(
        RoomEnemySlot slot,
        ushort graphicsVariant,
        ushort configurationPointer,
        ushort expectedCopyFunction,
        ushort expectedMoveFunction,
        ushort expectedGraphicsInitFunction)
    {
        int configurationAddress = 0xa90000 | configurationPointer;
        ushort tablePointer = ReadWord(_bus!, configurationAddress);
        ushort vramTablePointer = ReadWord(_bus!, configurationAddress + 2);
        ushort copyFunction = ReadWord(_bus!, configurationAddress + 4);
        ushort moveFunction = ReadWord(_bus!, configurationAddress + 6);
        ushort entryCount = ReadWord(_bus!, configurationAddress + 8);
        ushort graphicsInitFunction = ReadWord(_bus!, configurationAddress + 10);
        ushort rotationTablePointer = ReadWord(_bus!, configurationAddress + 12);
        ushort finishFunction = ReadWord(_bus!, configurationAddress + 14);

        if (copyFunction != expectedCopyFunction ||
            moveFunction != expectedMoveFunction ||
            graphicsInitFunction != expectedGraphicsInitFunction ||
            finishFunction != DeadMonsterFinishedFunction)
        {
            throw new InvalidDataException(
                $"Dead sidehopper configuration $A9:{configurationPointer:X4} selected " +
                $"callbacks ${copyFunction:X4}/${moveFunction:X4}/" +
                $"${graphicsInitFunction:X4}/${finishFunction:X4}.");
        }
        if (entryCount == 0)
            throw new InvalidDataException("Dead sidehopper corpse configuration has zero rows.");

        ushort yLimit = unchecked((ushort)(entryCount - 1));
        ushort lateMoveEntryIndex = unchecked((ushort)(yLimit - 1));
        ushort wrapOffset = unchecked((ushort)(
            ReadWord(_bus!, 0xa90000 | unchecked((ushort)(rotationTablePointer + 2))) - 12));
        var state = new DeadSidehopperEnemyState(
            slot,
            graphicsVariant,
            configurationPointer,
            tablePointer,
            vramTablePointer,
            copyFunction,
            moveFunction,
            rotationTablePointer,
            finishFunction,
            entryCount,
            yLimit,
            lateMoveEntryIndex,
            wrapOffset);
        _deadSidehopperStates[slot.SlotIndex] = state;

        CorpseRottingTableProcessor.Initialize(
            _bus!,
            0x7e0000 | tablePointer,
            entryCount);
        InitializeDeadSidehopperGraphics(graphicsVariant);
        return state;
    }

    /// <summary>Ports <c>DeadSidehopper_Main</c> at <c>$A9:D8DB</c>.</summary>
    private void RunDeadSidehopperMain(
        RoomEnemySlot slot,
        SamusState? samus,
        RoomLevelData? level,
        ushort cameraX)
    {
        DeadSidehopperEnemyState state = RequireDeadSidehopperState(slot);
        _deadSidehopperLevel = level;
        _deadSidehopperCameraX = cameraX;
        DispatchDeadSidehopperState(slot, state, samus, level, cameraX);
    }

    private void DispatchDeadSidehopperState(
        RoomEnemySlot slot,
        DeadSidehopperEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        ushort cameraX)
    {
        switch (state.Function)
        {
            case DeadSidehopperAiFunction.AliveWaitForCamera:
                if (unchecked((short)(cameraX - 513)) < 0)
                {
                    state.Function = DeadSidehopperAiFunction.ActivatedMovement;
                    goto case DeadSidehopperAiFunction.ActivatedMovement;
                }
                return;

            case DeadSidehopperAiFunction.ActivatedMovement:
                if (MoveDeadSidehopper(slot, state, level))
                {
                    state.JumpPhase = unchecked((ushort)((state.JumpPhase + 1) & 3));
                    SetDeadSidehopperInstruction(slot, DeadSidehopperLandingInstruction);
                    state.Function = DeadSidehopperAiFunction.NoOperation;
                }
                return;

            case DeadSidehopperAiFunction.NoOperation:
                return;

            case DeadSidehopperAiFunction.BeginPostLandingDelay:
                state.Function = DeadSidehopperAiFunction.PostLandingDelay;
                state.StateTimer = 64;
                return;

            case DeadSidehopperAiFunction.PostLandingDelay:
                state.StateTimer = unchecked((ushort)(state.StateTimer - 1));
                if ((state.StateTimer & 0x8000) == 0)
                    return;

                if (state.PaletteStage != 0)
                {
                    state.Function = DeadSidehopperAiFunction.TransformPalette;
                    return;
                }

                state.Function = DeadSidehopperAiFunction.ActivatedMovement;
                SetDeadSidehopperInstruction(slot, DeadSidehopperInitialInstruction);
                int tableIndex = state.JumpPhase * 2;
                state.VerticalVelocity = ReadWord(
                    _bus!, EnemyRomTablePointers.DeadSidehopper.VerticalVelocityWords + tableIndex);
                state.HorizontalVelocity = ReadWord(
                    _bus!, EnemyRomTablePointers.DeadSidehopper.HorizontalVelocityWords + tableIndex);
                return;

            case DeadSidehopperAiFunction.TransformPalette:
                StepDeadSidehopperPaletteTransformation(slot, state);
                return;

            case DeadSidehopperAiFunction.WaitForSamusCollision:
                if (samus?.Kinematics.DidCollideWithSolidEnemy(slot.NativeIndex) == true)
                    state.Function = DeadSidehopperAiFunction.PreRotDelay;
                return;

            case DeadSidehopperAiFunction.PreRotDelay:
                state.PreRotDelayCounter = unchecked((ushort)(state.PreRotDelayCounter + 1));
                if (state.PreRotDelayCounter >= 16)
                {
                    state.Function = DeadSidehopperAiFunction.Rotting;
                    slot.Properties = unchecked((ushort)(
                        slot.Properties | DeadMonsterInteractionRejectedProperty));
                }
                return;

            case DeadSidehopperAiFunction.Rotting:
                RunDeadSidehopperRotting(slot, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Dead sidehopper function $A9:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>
    /// Implements <c>DeadMonsters_Func_3/4</c> at <c>$A9:D961/D9C7</c>. The scripted
    /// Shitroid-room half moves directly through the staged set until X reaches 544; the
    /// general room half then switches to the shared terrain collision primitives.
    /// </summary>
    private bool MoveDeadSidehopper(
        RoomEnemySlot slot,
        DeadSidehopperEnemyState state,
        RoomLevelData? level)
    {
        bool beforeTerrainBoundary = unchecked((short)(slot.XPosition - 544)) < 0;
        int horizontalDisplacement = unchecked((short)state.HorizontalVelocity) << 8;
        if (beforeTerrainBoundary)
        {
            MoveDeadSidehopperAxisDirect(
                slot,
                horizontal: true,
                state.HorizontalVelocity);
        }
        else
        {
            MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                RequireDeadSidehopperLevel(level),
                slot,
                horizontalDisplacement);
        }

        // Upward motion accelerates by $20 in signed 8.8; falling motion accelerates by
        // $80. The sign test is performed before the addition in the cartridge.
        ushort acceleration = (state.VerticalVelocity & 0x8000) != 0
            ? (ushort)32
            : (ushort)128;
        state.VerticalVelocity = unchecked((ushort)(state.VerticalVelocity + acceleration));
        if (beforeTerrainBoundary)
        {
            MoveDeadSidehopperAxisDirect(
                slot,
                horizontal: false,
                state.VerticalVelocity);
            return unchecked((short)(slot.YPosition - 184)) >= 0;
        }

        int verticalDisplacement = unchecked((short)state.VerticalVelocity) << 8;
        return MoveEnemyVertically(
            RequireDeadSidehopperLevel(level),
            slot,
            verticalDisplacement);
    }

    private static void MoveDeadSidehopperAxisDirect(
        RoomEnemySlot slot,
        bool horizontal,
        ushort velocity)
    {
        ushort position = horizontal ? slot.XPosition : slot.YPosition;
        ushort subposition = horizontal ? slot.XSubposition : slot.YSubposition;
        int carry = (subposition >> 8) + (byte)velocity;
        subposition = unchecked((ushort)(
            (subposition & 0x00ff) | ((carry & 0xff) << 8)));
        position = unchecked((ushort)(
            position + unchecked((sbyte)(velocity >> 8)) + (carry >> 8)));
        if (horizontal)
        {
            slot.XPosition = position;
            slot.XSubposition = subposition;
        }
        else
        {
            slot.YPosition = position;
            slot.YSubposition = subposition;
        }
    }

    private static RoomLevelData RequireDeadSidehopperLevel(RoomLevelData? level) =>
        level ?? throw new InvalidDataException(
            "Dead sidehopper crossed into terrain-driven movement without room level data.");

    /// <summary>Ports palette transformation <c>$A9:DA08</c>.</summary>
    private void StepDeadSidehopperPaletteTransformation(
        RoomEnemySlot slot,
        DeadSidehopperEnemyState state)
    {
        state.PaletteFrameCounter = unchecked((ushort)(state.PaletteFrameCounter + 1));
        if (state.PaletteFrameCounter < 8)
            return;

        state.PaletteFrameCounter = 0;
        ushort sourcePointer = unchecked((ushort)(
            32 * (state.PaletteStage - 1) - 0x1434));
        for (int color = 0; color < 15; color++)
        {
            _cgram!.SetColor(
                0x91 + color,
                ReadWord(_bus!, 0xa90000 | unchecked((ushort)(sourcePointer + color * 2))));
        }

        state.PaletteStage = unchecked((ushort)(state.PaletteStage + 1));
        if (state.PaletteStage < 8)
            return;

        SetDeadSidehopperInstruction(slot, DeadSidehopperCorpseInstruction);
        state.Function = DeadSidehopperAiFunction.WaitForSamusCollision;
        slot.Properties = unchecked((ushort)(slot.Properties | DeadMonsterSolidProperty));
        slot.YRadius = 12;
    }

    /// <summary>
    /// Instruction <c>$A9:ECD0</c> selects the post-landing state from the companion's
    /// palette-activation word. It is deliberately instruction-driven: replacing it with a
    /// main-AI timer would make animation length and movement cadence diverge.
    /// </summary>
    private void SelectDeadSidehopperPostAnimationState(RoomEnemySlot slot)
    {
        DeadSidehopperEnemyState state = RequireDeadSidehopperState(slot);
        state.Function = state.PaletteStage != 0
            ? DeadSidehopperAiFunction.TransformPalette
            : DeadSidehopperAiFunction.BeginPostLandingDelay;
    }

    /// <summary>Writes Shitroid state 11's victim-owned extended word 08.</summary>
    private void ActivateDeadSidehopperVictim(RoomEnemySlot shitroid, ShitroidEnemyState state)
    {
        RoomEnemySlot victim = RequireShitroidVictim(shitroid);
        DeadSidehopperEnemyState victimState = RequireDeadSidehopperState(victim);
        victimState.PaletteStage = 1;
        state.VictimActivationFlag = 1;
    }

    /// <summary>Ports shot callback <c>$A9:DD1D</c>.</summary>
    private void ResolveDeadSidehopperShot(RoomEnemySlot slot)
    {
        DeadSidehopperEnemyState state = RequireDeadSidehopperState(slot);
        if ((slot.Properties & DeadMonsterInteractionRejectedProperty) != 0 ||
            state.PaletteStage < 8)
        {
            return;
        }
        TriggerDeadSidehopperRotting(slot, state);
    }

    /// <summary>Ports touch callback <c>$A9:DD44</c>.</summary>
    private void ResolveDeadSidehopperTouch(
        RoomEnemySlot slot,
        DeadSidehopperEnemyState state,
        SamusState samus,
        ushort controllerInput)
    {
        if (state.PaletteStage < 8)
        {
            ResolveNormalEnemyTouch(slot, samus, controllerInput);
            return;
        }
        TriggerDeadSidehopperRotting(slot, state);
    }

    /// <summary>Ports power-bomb callback <c>$A9:D8CC</c>.</summary>
    private void ResolveDeadSidehopperPowerBomb(RoomEnemySlot slot, SamusState? samus)
    {
        DeadSidehopperEnemyState state = RequireDeadSidehopperState(slot);
        if (state.PaletteStage >= 8)
        {
            ResolveDeadSidehopperShot(slot);
            return;
        }

        // Before the corpse is ready, the private power-bomb callback literally invokes
        // the actor's main dispatcher once. Use the last scheduled room context because the
        // outer bank-$A0 power-bomb pass does not carry level/camera arguments.
        DispatchDeadSidehopperState(
            slot,
            state,
            samus,
            _deadSidehopperLevel,
            _deadSidehopperCameraX);
    }

    private static void TriggerDeadSidehopperRotting(
        RoomEnemySlot slot,
        DeadSidehopperEnemyState state)
    {
        state.Function = DeadSidehopperAiFunction.Rotting;
        slot.Properties = slot.Properties.With(
            EnemyProperties.ProcessOffScreen | EnemyProperties.IgnoreSamusCollision);
    }

    private void RunDeadSidehopperRotting(
        RoomEnemySlot slot,
        DeadSidehopperEnemyState state)
    {
        state.ProcessCallCount++;
        bool stillRotting = CorpseRottingTableProcessor.Step(
            _bus!,
            0x7e0000 | state.TablePointer,
            state.EntryCount,
            state.YLimit,
            state.LateMoveEntryIndex,
            (yOffset, move) => CopyOrMoveDeadSidehopperPixelRow(state, yOffset, move),
            entryIndex => FinishDeadSidehopperCorpseRow(slot, state, entryIndex));
        if (!stillRotting)
            state.Function = DeadSidehopperAiFunction.WaitForSamusCollision;
        BuildDeadSidehopperVramTransfers(state);
    }

    private void InitializeDeadSidehopperGraphics(ushort graphicsVariant)
    {
        int variantIndex = graphicsVariant == 0 ? 0 : 1;
        foreach (DeadSidehopperGraphicsCopy copy in
                 DeadSidehopperInitialGraphicsCopies[variantIndex])
        {
            for (int byteIndex = 0; byteIndex < copy.Length; byteIndex++)
            {
                _bus!.WriteByte(
                    DeadMonsterWorkBufferAddress + copy.DestinationOffset + byteIndex,
                    _bus.ReadByte(
                        DeadMonsterTileDataAddress + copy.SourceOffset + byteIndex));
            }
        }
    }

    private void CopyOrMoveDeadSidehopperPixelRow(
        DeadSidehopperEnemyState state,
        ushort yOffset,
        bool move)
    {
        int rotationEntryAddress = 0xa90000 |
            unchecked((ushort)(state.RotationTablePointer + (yOffset >> 3) * 2));
        ushort sourceOffset = unchecked((ushort)(
            ReadWord(_bus!, rotationEntryAddress) + (yOffset & 7) * 2));
        ushort destinationOffset = (yOffset & 7) >= 6
            ? unchecked((ushort)(state.WrapOffset + sourceOffset))
            : sourceOffset;

        ReadOnlySpan<ushort> columnOffsets = state.GraphicsVariant == 0
            ? DeadSidehopperColumnWordOffsets0
            : DeadSidehopperColumnWordOffsets2;
        ReadOnlySpan<ushort> minimumY = state.GraphicsVariant == 0
            ? DeadSidehopperColumnMinimumY0
            : DeadSidehopperColumnMinimumY2;
        for (int columnIndex = 0; columnIndex < columnOffsets.Length; columnIndex++)
        {
            if (yOffset < minimumY[columnIndex])
                continue;

            int sourceWord = sourceOffset / 2 + columnOffsets[columnIndex];
            int destinationWord = destinationOffset / 2 + columnOffsets[columnIndex] + 1;
            if (yOffset < 38)
            {
                WriteDeadMonsterWorkWord(destinationWord, ReadDeadMonsterWorkWord(sourceWord));
                WriteDeadMonsterWorkWord(
                    destinationWord + 8,
                    ReadDeadMonsterWorkWord(sourceWord + 8));
            }

            if (move)
            {
                WriteDeadMonsterWorkWord(sourceWord, 0);
                WriteDeadMonsterWorkWord(sourceWord + 8, 0);
            }
        }
    }

    private void FinishDeadSidehopperCorpseRow(
        RoomEnemySlot slot,
        DeadSidehopperEnemyState state,
        ushort entryIndex)
    {
        state.FinishedEntryCount++;
        state.LastFinishedEntryIndex = entryIndex;
        ushort random = RequireRandomNumber();
        SpawnRoomGraphicsDustExplosion(
            unchecked((ushort)(slot.XPosition + (random & 0x001a) - 14)),
            unchecked((ushort)(slot.YPosition + 16)),
            animationIndex: 10);
        state.DustSpawnCount++;
        if ((_randomEnemyCounter & 7) == 0)
            LastDeadSidehopperSoundEffect = 0x0010;
    }

    private void BuildDeadSidehopperVramTransfers(DeadSidehopperEnemyState state)
    {
        ushort cursor = state.VramTablePointer;
        for (int recordIndex = 0; recordIndex < 64; recordIndex++, cursor += 8)
        {
            int address = 0xa90000 | cursor;
            ushort size = ReadWord(_bus!, address);
            if (size == 0)
                return;

            ushort sourceBankWord = ReadWord(_bus!, address + 2);
            ushort sourceOffset = ReadWord(_bus!, address + 4);
            ushort vramDestination = ReadWord(_bus!, address + 6);
            int sourceAddress = ((sourceBankWord & 0xff00) << 8) | sourceOffset;
            _deadSidehopperFrameVramTransfers.Add(new VramWriteEntry(
                size,
                sourceAddress,
                vramDestination));
        }

        throw new InvalidDataException(
            $"Dead sidehopper VRAM table $A9:{state.VramTablePointer:X4} has no terminator.");
    }

    private void QueueDeadSidehopperFrameVramTransfers(VramWriteQueue? queue)
    {
        if (queue is null)
            return;
        foreach (VramWriteEntry transfer in _deadSidehopperFrameVramTransfers)
        {
            queue.Enqueue(
                transfer.SizeInBytes,
                transfer.SourceAddress,
                transfer.EncodedVramDestination);
        }
    }

    private static void SetDeadSidehopperInstruction(RoomEnemySlot slot, ushort instruction)
    {
        slot.CurrentInstruction = instruction;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private DeadSidehopperEnemyState RequireDeadSidehopperState(RoomEnemySlot slot)
    {
        DeadSidehopperEnemyState state = _deadSidehopperStates[slot.SlotIndex] ??
            throw new InvalidDataException(
                $"Enemy slot {slot.SlotIndex} has no initialized dead-sidehopper state.");
        if (!ReferenceEquals(state.Slot, slot))
            throw new InvalidDataException("Dead-sidehopper state belongs to a different slot.");
        return state;
    }

    private ushort ReadDeadMonsterWorkWord(int wordOffset) =>
        ReadWord(_bus!, DeadMonsterWorkBufferAddress + wordOffset * 2);

    private void WriteDeadMonsterWorkWord(int wordOffset, ushort value) =>
        WriteWord(_bus!, DeadMonsterWorkBufferAddress + wordOffset * 2, value);

    private readonly record struct DeadSidehopperGraphicsCopy(
        int SourceOffset,
        int DestinationOffset,
        int Length);
}

/// <summary>Native bank-$A9 dead-sidehopper function pointers.</summary>
public enum DeadSidehopperAiFunction : ushort
{
    AliveWaitForCamera = 0xd8e2,
    ActivatedMovement = 0xd8f1,
    NoOperation = 0xd90f,
    BeginPostLandingDelay = 0xd910,
    PostLandingDelay = 0xd91d,
    TransformPalette = 0xda08,
    WaitForSamusCollision = 0xda64,
    PreRotDelay = 0xda8f,
    Rotting = 0xdaba,
}

/// <summary>Typed projection of one dead sidehopper's extended bank-$A9 WRAM.</summary>
public sealed class DeadSidehopperEnemyState
{
    internal DeadSidehopperEnemyState(
        RoomEnemySlot slot,
        ushort graphicsVariant,
        ushort configurationPointer,
        ushort tablePointer,
        ushort vramTablePointer,
        ushort copyFunction,
        ushort moveFunction,
        ushort rotationTablePointer,
        ushort finishFunction,
        ushort entryCount,
        ushort yLimit,
        ushort lateMoveEntryIndex,
        ushort wrapOffset)
    {
        Slot = slot;
        GraphicsVariant = graphicsVariant;
        ConfigurationPointer = configurationPointer;
        TablePointer = tablePointer;
        VramTablePointer = vramTablePointer;
        CopyFunction = copyFunction;
        MoveFunction = moveFunction;
        RotationTablePointer = rotationTablePointer;
        FinishFunction = finishFunction;
        EntryCount = entryCount;
        YLimit = yLimit;
        LateMoveEntryIndex = lateMoveEntryIndex;
        WrapOffset = wrapOffset;
    }

    public RoomEnemySlot Slot { get; }
    public ushort GraphicsVariant { get; }
    public ushort ConfigurationPointer { get; }
    public ushort TablePointer { get; }
    public ushort VramTablePointer { get; }
    public ushort CopyFunction { get; }
    public ushort MoveFunction { get; }
    public ushort RotationTablePointer { get; }
    public ushort FinishFunction { get; }
    public ushort EntryCount { get; }
    public ushort YLimit { get; }
    public ushort LateMoveEntryIndex { get; }
    public ushort WrapOffset { get; }

    public DeadSidehopperAiFunction Function
    {
        get => (DeadSidehopperAiFunction)Slot.VariableA;
        internal set => Slot.VariableA = (ushort)value;
    }

    /// <summary>Native extended word 06, selecting one of four jump velocity pairs.</summary>
    public ushort JumpPhase { get; internal set; }

    /// <summary>Native extended word 07, the eight-frame palette divider.</summary>
    public ushort PaletteFrameCounter { get; internal set; }

    /// <summary>Native extended word 08, written to one by Shitroid after feeding.</summary>
    public ushort PaletteStage { get; internal set; }

    /// <summary>Native signed 8.8 horizontal velocity word 0A.</summary>
    public ushort HorizontalVelocity { get; internal set; }

    /// <summary>Native signed 8.8 vertical velocity word 0B.</summary>
    public ushort VerticalVelocity { get; internal set; }

    /// <summary>Native AI variable B, deliberately retained across repeated corpse contacts.</summary>
    public ushort PreRotDelayCounter
    {
        get => Slot.VariableB;
        internal set => Slot.VariableB = value;
    }

    /// <summary>Native AI variable F used by the 64-frame post-landing pause.</summary>
    public ushort StateTimer
    {
        get => Slot.VariableF;
        internal set => Slot.VariableF = value;
    }

    public uint ProcessCallCount { get; internal set; }
    public uint FinishedEntryCount { get; internal set; }
    public uint DustSpawnCount { get; internal set; }
    public ushort LastFinishedEntryIndex { get; internal set; } = ushort.MaxValue;
}
