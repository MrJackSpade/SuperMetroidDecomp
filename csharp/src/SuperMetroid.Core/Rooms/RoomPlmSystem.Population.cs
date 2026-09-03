using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Cartridge-authentic construction of bank-$84 room PLMs from a bank-$8F population.
/// </summary>
public sealed partial class RoomPlmSystem
{
    /// <summary>
    /// Returns whether a room-authored header has a setup owner in the sequential loader.
    /// This is a read-only inventory seam for private-ROM audits; production still performs
    /// dispatch and contextual failure from <see cref="LoadRoomPopulation"/> itself.
    /// </summary>
    public static bool IsSupportedRoomPopulationHeader(ushort header)
    {
        if (header is RoomPlmHeaders.ScrollTrigger or RightwardsExtensionHeader or
            LeftwardsExtensionHeader or DownwardsExtensionHeader or UpwardsExtensionHeader or
            MotherBrainGlassHeader or BombTorizoHandHeader or
            MapStationHeader or EnergyStationHeader or MissileStationHeader or
            ElevatorPlatformHeader or SaveStationHeader)
        {
            return true;
        }
        if (IsColoredDoorHeader(header) || TryIdentifyGreyDoor(header, out _))
            return true;
        return TryIdentifyPermanentCollectible(header, out _, out _);
    }

    /// <summary>
    /// Parses one zero-terminated room population exactly once, in increasing ROM-record
    /// order. Each record is allocated before its setup routine runs, matching
    /// <c>Spawn_Room_PLM</c> at <c>$84:846A</c>. A setup may immediately delete its slot;
    /// the next record can consequently reuse that same highest native slot.
    /// </summary>
    public int LoadRoomPopulation(
        ISnesAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        SnesVram vram,
        ushort populationPointer,
        Bank80SystemState system,
        byte areaIndex,
        Func<SamusState?> getSamus,
        Func<bool> isAreaTorizoDefeated,
        Func<bool>? isTourianStatueFinished = null,
        Func<byte, bool>? hasAreaBossBit = null,
        Func<int, bool>? hasEvent = null,
        Action<int>? setEvent = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(streamer);
        ArgumentNullException.ThrowIfNull(vram);
        ArgumentNullException.ThrowIfNull(system);
        ArgumentNullException.ThrowIfNull(getSamus);
        ArgumentNullException.ThrowIfNull(isAreaTorizoDefeated);

        // These are room-owned services used later by resident pre-instructions. They are
        // installed once, not once per family scan, so every actor in this population sees
        // the same cartridge/runtime state owner.
        _coloredDoorSystem = system;
        _greyDoorSystem = system;
        _greyDoorAreaIndex = areaIndex;
        _isTourianStatueFinished = isTourianStatueFinished;
        _collectibleSystem = system;
        _collectibleSamus = getSamus;
        _bombTorizoSamus = getSamus;
        _motherBrainHasAreaBossBit = hasAreaBossBit;
        _motherBrainHasEvent = hasEvent;
        _motherBrainSetEvent = setEvent;

        ushort cursor = populationPointer;
        int spawnedRecordCount = 0;
        for (int recordIndex = 0; recordIndex < 256; recordIndex++)
        {
            ushort header = ReadBank8fWord(bus, cursor);
            if (header == 0)
                return spawnedRecordCount;

            var record = new RoomPlmPopulationRecord(
                PopulationPointer: populationPointer,
                RecordIndex: recordIndex,
                RecordPointer: cursor,
                HeaderPointer: header,
                BlockX: bus.ReadByte(0x8f0000 | unchecked((ushort)(cursor + 2))),
                BlockY: bus.ReadByte(0x8f0000 | unchecked((ushort)(cursor + 3))),
                RoomArgument: ReadBank8fWord(bus, unchecked((ushort)(cursor + 4))));
            cursor = unchecked((ushort)(cursor + 6));

            int blockIndex;
            try
            {
                blockIndex = level.GetBlockIndex(record.BlockX, record.BlockY);
            }
            catch (ArgumentOutOfRangeException error)
            {
                throw new InvalidDataException(
                    $"Room PLM population $8F:{populationPointer:X4} record {recordIndex} " +
                    $"header $84:{header:X4} has out-of-room block " +
                    $"({record.BlockX},{record.BlockY}) and argument ${record.RoomArgument:X4}.",
                    error);
            }

            PlmSlot? slot = AllocateRoomPopulationSlot(bus, record, blockIndex);
            if (slot is null)
                continue; // The native routine returns carry set when all forty IDs are live.

            if (!TryRunRoomPopulationSetup(
                    bus,
                    level,
                    streamer,
                    vram,
                    system,
                    areaIndex,
                    getSamus,
                    isAreaTorizoDefeated,
                    record,
                    slot))
            {
                // Do not leave the preallocated but untranslated actor resident. The load is
                // intentionally loud and includes every value needed to locate the exact ROM
                // record without relying on a room-name special case.
                ClearSlot(slot);
                ushort setupPointer = ReadBank84Word(bus, header);
                ushort instructionList = ReadBank84Word(
                    bus,
                    unchecked((ushort)(header + 2)));
                throw new NotSupportedException(
                    $"Room PLM population $8F:{populationPointer:X4} record {recordIndex} " +
                    $"at $8F:{record.RecordPointer:X4} uses untranslated header " +
                    $"$84:{header:X4} (setup $84:{setupPointer:X4}, list " +
                    $"$84:{instructionList:X4}) at block ({record.BlockX},{record.BlockY}) " +
                    $"with argument ${record.RoomArgument:X4}.");
            }

            spawnedRecordCount++;
        }

        throw new InvalidDataException(
            $"Room PLM population $8F:{populationPointer:X4} has no zero terminator.");
    }

    /// <summary>Physical-slot views in cartridge handler order, highest ID first.</summary>
    public IReadOnlyList<RoomPlmSlotSnapshot> PopulationSlots => _slots
        .Select((slot, index) => (slot, index))
        .Where(entry => entry.slot.Active)
        .OrderByDescending(entry => entry.index)
        .Select(entry => new RoomPlmSlotSnapshot(
            NativeSlotIndex: entry.index,
            HeaderPointer: entry.slot.HeaderPointer,
            BlockIndex: entry.slot.BlockIndex,
            RoomArgument: entry.slot.RoomArgument,
            InstructionPointer: entry.slot.InstructionPointer))
        .ToArray();

    private PlmSlot? AllocateRoomPopulationSlot(
        ISnesAddressSpace bus,
        RoomPlmPopulationRecord record,
        int blockIndex)
    {
        for (int index = _slots.Length - 1; index >= 0; index--)
        {
            PlmSlot slot = _slots[index];
            if (slot.Active)
                continue;

            ClearSlot(slot);
            slot.Active = true;
            slot.HeaderPointer = record.HeaderPointer;
            slot.BlockIndex = blockIndex;
            slot.InstructionPointer = ReadBank84Word(
                bus,
                unchecked((ushort)(record.HeaderPointer + 2)));
            slot.InstructionTimer = 1;
            slot.RoomArgument = record.RoomArgument;
            return slot;
        }

        return null;
    }

    private static void ClearSlot(PlmSlot slot)
    {
        // Native PLM arrays contain only scalar words, so allocating an inactive ID
        // inherently replaces every prior family discriminator. The C# translation adds
        // semantic references for debugger clarity; every population and runtime spawn
        // must pass through this reset or a recycled bomb/door block can execute the old
        // item's handler before its own instruction stream.
        slot.Active = false;
        slot.HeaderPointer = 0;
        slot.BlockIndex = 0;
        slot.RestoreLevelWord = 0;
        slot.InstructionPointer = 0;
        slot.InstructionTimer = 0;
        slot.PreInstruction = 0;
        slot.RoomArgument = 0;
        slot.LoopTimer = 0;
        slot.Item = null;
        slot.Scroll = null;
        slot.ColoredDoor = null;
        slot.GreyDoor = null;
        slot.Station = null;
        slot.IsElevatorPlatform = false;
        slot.Treadmill = null;
    }

    private bool TryRunRoomPopulationSetup(
        ISnesAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        SnesVram vram,
        Bank80SystemState system,
        byte areaIndex,
        Func<SamusState?> getSamus,
        Func<bool> isAreaTorizoDefeated,
        RoomPlmPopulationRecord record,
        PlmSlot slot)
    {
        ushort header = record.HeaderPointer;
        if (TryIdentifyColoredDoor(
                header,
                out ColoredDoorColor color,
                out ColoredDoorOrientation coloredOrientation))
        {
            SetupColoredDoorSlot(bus, level, system, slot, color, coloredOrientation);
            return true;
        }

        if (TryIdentifyGreyDoor(header, out ColoredDoorOrientation greyOrientation))
        {
            SetupGreyDoorSlot(bus, level, system, slot, greyOrientation);
            return true;
        }

        if (header is RoomPlmHeaders.ScrollTrigger or RightwardsExtensionHeader or
            LeftwardsExtensionHeader or DownwardsExtensionHeader or UpwardsExtensionHeader)
        {
            SetupScrollSlot(level, slot, header);
            return true;
        }

        if (TryIdentifyPermanentCollectible(
                header,
                out InWorldCollectibleKind kind,
                out CollectiblePresentation presentation))
        {
            SetupCollectibleSlot(bus, level, streamer, vram, system, slot, kind, presentation);
            return true;
        }

        if (header == MotherBrainGlassHeader)
        {
            SetupMotherBrainGlassSlot(level, streamer, record, slot);
            return true;
        }

        if (header == BombTorizoHandHeader)
        {
            SetupBombTorizoHandSlot(level, slot, isAreaTorizoDefeated);
            return true;
        }

        if (TrySetupStationOrElevator(level, streamer, system, areaIndex, slot))
            return true;

        return false;
    }
}

/// <summary>One immutable six-byte record decoded from a bank-$8F room population.</summary>
public readonly record struct RoomPlmPopulationRecord(
    ushort PopulationPointer,
    int RecordIndex,
    ushort RecordPointer,
    ushort HeaderPointer,
    byte BlockX,
    byte BlockY,
    ushort RoomArgument);

/// <summary>Debugger/test view of one occupied physical PLM slot.</summary>
public readonly record struct RoomPlmSlotSnapshot(
    int NativeSlotIndex,
    ushort HeaderPointer,
    int BlockIndex,
    ushort RoomArgument,
    ushort InstructionPointer);
