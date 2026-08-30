using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Retail Dead Torizo actor and its bank-$A9 corpse-rotting graphics path.</summary>
public sealed partial class RoomEnemySystem
{
    public const ushort DeadTorizoDefinition = 0xed3f;

    private const ushort DeadTorizoSolidProperty = 0x8000;
    private const ushort DeadTorizoInitialInstruction = 0xd6dc;
    private const ushort DeadTorizoWaitFunction = 0xd3ad;
    private const ushort DeadTorizoPreRotFunction = 0xd3c8;
    private const ushort DeadTorizoRottingFunction = 0xd3e6;
    private const ushort DeadTorizoNoOperationFunction = 0xd3c7;
    private const ushort DeadTorizoHitbox = 0xd77c;
    private const ushort DeadTorizoHookSpritemap = 0xd761;
    private const ushort DeadTorizoCorpseConfiguration = 0xdd58;
    private const ushort DeadTorizoCopyFunction = 0xe38b;
    private const ushort DeadTorizoMoveFunction = 0xe272;
    private const ushort DeadTorizoGraphicsInitFunction = 0xde18;
    private const ushort DeadTorizoFinishedFunction = 0xd5bd;
    private const ushort DeadTorizoOddVramTable = 0xd583;
    private const ushort DeadTorizoEvenVramTable = 0xd549;
    private const int DeadTorizoWorkBufferAddress = 0x7e2000;
    private const int DeadTorizoWorkBufferSize = 0x1000;
    private const int DeadTorizoSandBufferAddress = 0x7e9500;
    private const int DeadTorizoTileDataAddress = 0xb7a800;

    private static readonly DeadTorizoGraphicsCopy[] DeadTorizoInitialGraphicsCopies =
    [
        new(0x0120, 0x0060, 0x00c0),
        new(0x0320, 0x01a0, 0x00c0),
        new(0x0500, 0x02c0, 0x0100),
        new(0x0700, 0x0400, 0x0100),
        new(0x0900, 0x0540, 0x0100),
        new(0x0b00, 0x0680, 0x0100),
        new(0x0d00, 0x07c0, 0x0100),
        new(0x0f00, 0x0900, 0x0100),
        new(0x1100, 0x0a40, 0x0100),
        new(0x12e0, 0x0b60, 0x0120),
        new(0x14c0, 0x0c80, 0x0140),
        new(0x16c0, 0x0dc0, 0x0140),
    ];

    // These are word offsets into the 4bpp staging surface. The paired planes-2/3 word is
    // eight words later, exactly matching `$A9:E272/$E38B`'s repeated indexed accesses.
    private static ReadOnlySpan<ushort> DeadTorizoColumnWordOffsets =>
        [0, 16, 32, 48, 64, 80, 96, 112, 128, 144];

    private static ReadOnlySpan<ushort> DeadTorizoColumnMinimumY =>
        [0x50, 0x48, 0x10, 0, 0, 0, 0, 0, 0, 0x10];

    private readonly List<VramWriteEntry> _deadTorizoFrameVramTransfers = [];
    private DeadTorizoEnemyState? _deadTorizo;

    /// <summary>The loaded Dead Torizo's typed extended-WRAM state.</summary>
    public DeadTorizoEnemyState? DeadTorizo => _deadTorizo;

    /// <summary>Alternating VRAM records authored by the most recent actor frame.</summary>
    public IReadOnlyList<VramWriteEntry> LastDeadTorizoVramTransfers =>
        _deadTorizoFrameVramTransfers;

    /// <summary>Last library-two sound emitted by a completed corpse row.</summary>
    public ushort? LastDeadTorizoSoundEffect { get; private set; }

    private void ResetDeadTorizoRoomState()
    {
        _deadTorizo = null;
        _deadTorizoFrameVramTransfers.Clear();
        LastDeadTorizoSoundEffect = null;
    }

    /// <summary>Ports <c>DeadTorizo_Init</c> at <c>$A9:D308</c>.</summary>
    private void InitializeDeadTorizo(RoomEnemySlot slot)
    {
        if (slot.SlotIndex != 0)
        {
            throw new InvalidDataException(
                $"Dead Torizo requires native slot zero, not slot {slot.SlotIndex}.");
        }

        for (int offset = 0; offset < DeadTorizoWorkBufferSize; offset++)
            _bus!.WriteByte(DeadTorizoWorkBufferAddress + offset, 0);

        slot.VariableA = DeadTorizoWaitFunction;
        slot.Properties = unchecked((ushort)(slot.Properties | 0xa000));
        slot.CurrentInstruction = DeadTorizoInitialInstruction;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.PaletteIndex = 0x0200;
        slot.VariableB = 0;
        slot.VariableC = 8;

        int config = 0xa90000 | DeadTorizoCorpseConfiguration;
        ushort tablePointer = ReadWord(_bus!, config);
        ushort vramTablePointer = ReadWord(_bus!, config + 2);
        ushort copyFunction = ReadWord(_bus!, config + 4);
        ushort moveFunction = ReadWord(_bus!, config + 6);
        ushort entryCount = ReadWord(_bus!, config + 8);
        ushort initFunction = ReadWord(_bus!, config + 10);
        ushort rotationTablePointer = ReadWord(_bus!, config + 12);
        ushort finishFunction = ReadWord(_bus!, config + 14);

        if (copyFunction != DeadTorizoCopyFunction ||
            moveFunction != DeadTorizoMoveFunction ||
            initFunction != DeadTorizoGraphicsInitFunction ||
            finishFunction != DeadTorizoFinishedFunction)
        {
            throw new InvalidDataException(
                $"Dead Torizo corpse configuration $A9:{DeadTorizoCorpseConfiguration:X4} " +
                $"selected callbacks ${copyFunction:X4}/${moveFunction:X4}/" +
                $"${initFunction:X4}/${finishFunction:X4}.");
        }

        ushort yLimit = unchecked((ushort)(entryCount - 1));
        ushort lateMoveEntryIndex = unchecked((ushort)(yLimit - 1));
        ushort wrapOffset = unchecked((ushort)(
            ReadWord(_bus!, 0xa90000 | unchecked((ushort)(rotationTablePointer + 2))) - 12));
        _deadTorizo = new DeadTorizoEnemyState(
            slot,
            tablePointer,
            vramTablePointer,
            copyFunction,
            moveFunction,
            rotationTablePointer,
            finishFunction,
            entryCount,
            yLimit,
            lateMoveEntryIndex,
            wrapOffset)
        {
            SandLineCounter = 15,
        };

        CorpseRottingTableProcessor.Initialize(
            _bus!,
            0x7e0000 | tablePointer,
            entryCount);
        InitializeDeadTorizoGraphics();
    }

    /// <summary>Ports <c>DeadTorizo_Main</c> at <c>$A9:D368</c>.</summary>
    private void RunDeadTorizoMain(RoomEnemySlot slot, SamusState? samus)
    {
        DeadTorizoEnemyState state = RequireDeadTorizoState(slot);

        // This authored multi-rectangle detector is enabled only while property `$8000` is
        // clear. It writes external Samus displacement before selecting immediate rotting.
        if ((slot.Properties & DeadTorizoSolidProperty) == 0 &&
            samus is not null &&
            DeadTorizoCustomHitboxOverlaps(slot, samus))
        {
            TriggerDeadTorizoRotting(slot);
        }

        switch (slot.VariableA)
        {
            case DeadTorizoWaitFunction:
                if (samus?.Kinematics.DidCollideWithSolidEnemy(slot.NativeIndex) == true)
                    slot.VariableA = DeadTorizoPreRotFunction;
                break;

            case DeadTorizoPreRotFunction:
                state.PreRotDelayCounter = unchecked((ushort)(state.PreRotDelayCounter + 1));
                if (state.PreRotDelayCounter >= 0x10)
                {
                    slot.Properties = unchecked((ushort)(
                        slot.Properties | DeadTorizoSolidProperty));
                    slot.VariableA = DeadTorizoRottingFunction;
                    RunDeadTorizoRotting(slot, state);
                }
                break;

            case DeadTorizoRottingFunction:
                RunDeadTorizoRotting(slot, state);
                break;

            case DeadTorizoNoOperationFunction:
                break;

            default:
                throw new NotSupportedException(
                    $"Dead Torizo function $A9:{slot.VariableA:X4} is not translated.");
        }

        BuildDeadTorizoVramTransfers(state);
    }

    private void RunDeadTorizoRotting(RoomEnemySlot slot, DeadTorizoEnemyState state)
    {
        state.SandFrameCounter = unchecked((ushort)(state.SandFrameCounter + 1));
        if (state.SandFrameCounter >= 0x0f)
        {
            state.SandFrameCounter = 0;
            if (state.SandLineCounter != 0)
            {
                CopyDeadTorizoSandLine(state.SandLineCounter);
                state.SandLineCounter = unchecked((ushort)(state.SandLineCounter - 1));
                state.SandLineCopyCount++;
            }
        }

        slot.VariableC = unchecked((ushort)(slot.VariableC + 1));
        MoveBankA9EnemyWithVelocity(slot);
        state.ProcessCallCount++;
        bool stillRotting = CorpseRottingTableProcessor.Step(
            _bus!,
            0x7e0000 | state.TablePointer,
            state.EntryCount,
            state.YLimit,
            state.LateMoveEntryIndex,
            (yOffset, move) => CopyOrMoveDeadTorizoPixelRow(state, yOffset, move),
            entryIndex => FinishDeadTorizoCorpseRow(state, entryIndex));
        if (!stillRotting)
            slot.VariableA = DeadTorizoNoOperationFunction;
    }

    /// <summary>Shot/touch tail at <c>$A9:D433</c>.</summary>
    private void TriggerDeadTorizoRotting(RoomEnemySlot slot)
    {
        RequireDeadTorizoState(slot);
        slot.Properties = unchecked((ushort)(slot.Properties | DeadTorizoSolidProperty));
        slot.VariableA = DeadTorizoRottingFunction;
    }

    /// <summary>Power-bomb entry at <c>$A9:D42A</c>.</summary>
    private void TriggerDeadTorizoPowerBomb(RoomEnemySlot slot)
    {
        RequireDeadTorizoState(slot);
        if ((slot.Properties & DeadTorizoSolidProperty) == 0)
            TriggerDeadTorizoRotting(slot);
    }

    private bool DeadTorizoCustomHitboxOverlaps(RoomEnemySlot slot, SamusState samus)
    {
        SamusKinematicsState kinematics = samus.Kinematics;
        int cursor = 0xa90000 | DeadTorizoHitbox;
        ushort hitboxCount = ReadWord(_bus!, cursor);
        cursor += 2;

        for (int hitboxIndex = 0; hitboxIndex < hitboxCount; hitboxIndex++, cursor += 8)
        {
            ushort verticalDistance;
            ushort verticalRadius;
            if (unchecked((short)(kinematics.YPosition - slot.YPosition)) >= 0)
            {
                verticalDistance = unchecked((ushort)(kinematics.YPosition - slot.YPosition));
                verticalRadius = ReadWord(_bus!, cursor + 6);
            }
            else
            {
                verticalDistance = unchecked((ushort)(slot.YPosition - kinematics.YPosition));
                verticalRadius = ReadWord(_bus!, cursor + 2);
            }

            ushort verticalOverlap = unchecked((ushort)(
                kinematics.YRadius + NativeAbsolute(verticalRadius) - verticalDistance));
            if (unchecked((short)verticalOverlap) < 0)
                continue;

            ushort horizontalDistance;
            ushort horizontalRadius;
            if (unchecked((short)(kinematics.XPosition - slot.XPosition)) >= 0)
            {
                horizontalDistance = unchecked((ushort)(kinematics.XPosition - slot.XPosition));
                horizontalRadius = ReadWord(_bus!, cursor + 4);
            }
            else
            {
                horizontalDistance = unchecked((ushort)(slot.XPosition - kinematics.XPosition));
                horizontalRadius = ReadWord(_bus!, cursor);
            }

            short overlap = unchecked((short)(
                kinematics.XRadius + NativeAbsolute(horizontalRadius) - horizontalDistance));
            if (overlap < 0)
                continue;
            if (overlap < 4)
                overlap = 4;

            kinematics.ExtraXDisplacement = unchecked((ushort)overlap);
            kinematics.ExtraYDisplacement = 4;
            kinematics.ExtraXSubdisplacement = 0;
            kinematics.ExtraYSubdisplacement = 0;
            return true;
        }

        return false;
    }

    private void InitializeDeadTorizoGraphics()
    {
        foreach (DeadTorizoGraphicsCopy copy in DeadTorizoInitialGraphicsCopies)
        {
            for (int byteIndex = 0; byteIndex < copy.Length; byteIndex++)
            {
                _bus!.WriteByte(
                    DeadTorizoWorkBufferAddress + copy.DestinationOffset + byteIndex,
                    _bus.ReadByte(DeadTorizoTileDataAddress + copy.SourceOffset + byteIndex));
            }
        }
    }

    private void CopyOrMoveDeadTorizoPixelRow(
        DeadTorizoEnemyState state,
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

        ReadOnlySpan<ushort> columnOffsets = DeadTorizoColumnWordOffsets;
        ReadOnlySpan<ushort> minimumY = DeadTorizoColumnMinimumY;
        for (int columnIndex = 0; columnIndex < columnOffsets.Length; columnIndex++)
        {
            if (yOffset < minimumY[columnIndex])
                continue;

            int sourceWord = sourceOffset / 2 + columnOffsets[columnIndex];
            int destinationWord = destinationOffset / 2 + columnOffsets[columnIndex] + 1;
            if (yOffset < 94)
            {
                WriteWorkWord(destinationWord, ReadWorkWord(sourceWord));
                WriteWorkWord(destinationWord + 8, ReadWorkWord(sourceWord + 8));
            }

            if (move)
            {
                WriteWorkWord(sourceWord, 0);
                WriteWorkWord(sourceWord + 8, 0);
            }
        }
    }

    private void FinishDeadTorizoCorpseRow(DeadTorizoEnemyState state, ushort entryIndex)
    {
        state.FinishedEntryCount++;
        state.LastFinishedEntryIndex = entryIndex;
        ushort random = _readRandomNumber?.Invoke() ?? 0;
        SpawnRoomGraphicsDustExplosion(
            unchecked((ushort)((random & 0x001f) + 272)),
            188,
            animationIndex: 10);
        state.DustSpawnCount++;
        if ((_randomEnemyCounter & 7) == 0)
            LastDeadTorizoSoundEffect = 0x0010;
    }

    private void CopyDeadTorizoSandLine(ushort lineIndex)
    {
        ushort destinationOffset = ReadWord(
            _bus!,
            0xa9d67c + lineIndex * 2);
        ushort sourceOffset = ReadWord(
            _bus!,
            0xa9d69c + lineIndex * 2);

        // Eighteen tile rows are sixteen bytes apart in both the source sheet and the WRAM
        // heap surface. Only the first 16-bit bitplane word of each row is replaced.
        for (int row = 0; row < 18; row++)
        {
            ushort value = ReadWord(
                _bus!,
                DeadTorizoTileDataAddress + sourceOffset + row * 16);
            WriteWord(
                _bus!,
                DeadTorizoSandBufferAddress + destinationOffset + row * 16,
                value);
        }
    }

    /// <summary>
    /// Applies bank-$A9's shared signed 8.8 enemy velocity pair. Dead Torizo and Shitroid
    /// both call the same cartridge helper; retaining one implementation also makes its
    /// unusual use of the high byte of the subposition explicit.
    /// </summary>
    private static void MoveBankA9EnemyWithVelocity(RoomEnemySlot slot)
    {
        int xCarry = (slot.XSubposition >> 8) + (byte)slot.VariableB;
        slot.XSubposition = unchecked((ushort)(
            (slot.XSubposition & 0x00ff) | ((xCarry & 0xff) << 8)));
        slot.XPosition = unchecked((ushort)(
            slot.XPosition + unchecked((sbyte)(slot.VariableB >> 8)) + (xCarry >> 8)));

        int yCarry = (slot.YSubposition >> 8) + (byte)slot.VariableC;
        slot.YSubposition = unchecked((ushort)(
            (slot.YSubposition & 0x00ff) | ((yCarry & 0xff) << 8)));
        slot.YPosition = unchecked((ushort)(
            slot.YPosition + unchecked((sbyte)(slot.VariableC >> 8)) + (yCarry >> 8)));
    }

    private void BuildDeadTorizoVramTransfers(DeadTorizoEnemyState state)
    {
        state.VramTransferPhase = unchecked((ushort)(state.VramTransferPhase + 1));
        ushort cursor = (state.VramTransferPhase & 1) != 0
            ? DeadTorizoOddVramTable
            : DeadTorizoEvenVramTable;

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
            _deadTorizoFrameVramTransfers.Add(new VramWriteEntry(
                size,
                sourceAddress,
                vramDestination));
        }

        throw new InvalidDataException(
            $"Dead Torizo VRAM table $A9:{cursor:X4} has no zero-size terminator.");
    }

    private void QueueDeadTorizoFrameVramTransfers(VramWriteQueue? queue)
    {
        if (queue is null)
            return;
        foreach (VramWriteEntry transfer in _deadTorizoFrameVramTransfers)
        {
            queue.Enqueue(
                transfer.SizeInBytes,
                transfer.SourceAddress,
                transfer.EncodedVramDestination);
        }
    }

    /// <summary>Runs the post-enemy ordinary-spritemap hook at <c>$A9:D39A</c>.</summary>
    private void DrawDeadTorizoHook(OamBuffer oam, ushort cameraX, ushort cameraY)
    {
        if (_deadTorizo is null)
            return;

        ushort screenY = unchecked((ushort)(187 - cameraY));
        if (unchecked((short)screenY) < 0)
            return;
        oam.AddEnemySpritemap(
            _bus!,
            bank: 0xa9,
            spritemapPointer: DeadTorizoHookSpritemap,
            unchecked((ushort)(296 - cameraX)),
            screenY,
            paletteBits: 0,
            baseTileIndex: 0);
    }

    private DeadTorizoEnemyState RequireDeadTorizoState(RoomEnemySlot slot)
    {
        DeadTorizoEnemyState state = _deadTorizo ??
            throw new InvalidDataException("Dead Torizo has no initialized extended state.");
        if (!ReferenceEquals(state.Slot, slot))
            throw new InvalidDataException("A non-Dead-Torizo slot entered its dispatcher.");
        return state;
    }

    private ushort ReadWorkWord(int wordOffset) =>
        ReadWord(_bus!, DeadTorizoWorkBufferAddress + wordOffset * 2);

    private void WriteWorkWord(int wordOffset, ushort value) =>
        WriteWord(_bus!, DeadTorizoWorkBufferAddress + wordOffset * 2, value);

    private static ushort NativeAbsolute(ushort value) =>
        (value & 0x8000) == 0 ? value : unchecked((ushort)(~value + 1));

    private static void WriteWord(ISnesAddressSpace bus, int address, ushort value)
    {
        bus.WriteByte(address, unchecked((byte)value));
        bus.WriteByte(address + 1, unchecked((byte)(value >> 8)));
    }

    private readonly record struct DeadTorizoGraphicsCopy(
        int SourceOffset,
        int DestinationOffset,
        int Length);
}

/// <summary>Typed projection of Dead Torizo's bank-$A9 extended WRAM fields.</summary>
public sealed class DeadTorizoEnemyState
{
    internal DeadTorizoEnemyState(
        RoomEnemySlot slot,
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
    public ushort VramTransferPhase { get; internal set; }
    public ushort PreRotDelayCounter { get; internal set; }
    public ushort SandLineCounter { get; internal set; }
    public ushort SandFrameCounter { get; internal set; }
    public uint ProcessCallCount { get; internal set; }
    public uint FinishedEntryCount { get; internal set; }
    public uint DustSpawnCount { get; internal set; }
    public uint SandLineCopyCount { get; internal set; }
    public ushort LastFinishedEntryIndex { get; internal set; } = ushort.MaxValue;
}
