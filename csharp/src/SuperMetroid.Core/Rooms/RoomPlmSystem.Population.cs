using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Cartridge-authentic construction of bank-$84 room PLMs from compiled retail
/// bank-$8F placements or an explicitly supplied synthetic population.
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
        if (header is RoomPlmHeaders.ScrollTrigger or
            RoomPlmHeaders.RightwardsScrollExtension or
            RoomPlmHeaders.LeftwardsScrollExtension or
            RoomPlmHeaders.DownwardsScrollExtension or
            RoomPlmHeaders.UpwardsScrollExtension or
            RoomPlmHeaders.MotherBrainGlass or RoomPlmHeaders.BombTorizoHand or
            RoomPlmHeaders.MapStation or RoomPlmHeaders.EnergyStation or
            RoomPlmHeaders.MissileStation or RoomPlmHeaders.ElevatorPlatform or
            RoomPlmHeaders.SaveStation or
            RoomPlmHeaders.SpeedBoosterEscape or
            RoomPlmHeaders.WreckedShipAttic or
            RoomPlmHeaders.NoobTube or
            RoomPlmHeaders.SetMetroidsClearedStatesWhenRequired or
            RoomPlmHeaders.MotherBrainEscapeRoomGate or
            RoomPlmHeaders.DownwardGate or RoomPlmHeaders.DownwardGateShotBlock ||
            IsEyeDoorHeader(header) || IsDraygonCannonHeader(header))
        {
            return true;
        }
        if (IsColoredDoorHeader(header) || TryIdentifyGreyDoor(header, out _))
            return true;
        return TryIdentifyPermanentCollectible(header, out _, out _);
    }

    /// <summary>
    /// Allocates one decoded room population exactly once, in increasing source-record
    /// order. Each record is allocated before its setup routine runs, matching
    /// <c>Spawn_Room_PLM</c> at <c>$84:846A</c>. A setup may immediately delete its slot;
    /// the next record can consequently reuse that same highest native slot. Retail
    /// loads require decoded records; no population or header can be read from a bus.
    /// </summary>
    public int LoadRoomPopulation(
        ISnesAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        SnesVram vram,
        RoomPlmPopulationDefinition population,
        Bank80SystemState system,
        AreaId areaIndex,
        Func<SamusState?> getSamus,
        Func<bool> isAreaTorizoDefeated,
        Func<bool>? isTourianStatueFinished = null,
        Func<BossBits, bool>? hasAreaBossBit = null,
        Func<EventNumber, bool>? hasEvent = null,
        Action<EventNumber>? setEvent = null,
        RoomLayer3FxState? roomFx = null,
        Action<ushort>? setEarthquakeTimer = null,
        Action<ushort>? setEarthquakeType = null,
        Action<NoobTubeProjectileRequest>? spawnNoobTubeProjectile = null,
        Action<EyeDoorProjectileRequest>? spawnEyeDoorProjectile = null,
        Action<ushort>? disableDraygonCannon = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(streamer);
        ArgumentNullException.ThrowIfNull(vram);
        ArgumentNullException.ThrowIfNull(system);
        ArgumentNullException.ThrowIfNull(population);
        ArgumentNullException.ThrowIfNull(getSamus);
        ArgumentNullException.ThrowIfNull(isAreaTorizoDefeated);

        // These are room-owned services used later by resident pre-instructions. They are
        // installed once, not once per family scan, so every actor in this population sees
        // the same cartridge/runtime state owner.
        _coloredDoorSystem = system;
        _greyDoorSystem = system;
        _greyDoorArea = areaIndex;
        _activeAreaIndex = areaIndex;
        _isTourianStatueFinished = isTourianStatueFinished;
        _collectibleSystem = system;
        _collectibleSamus = getSamus;
        _bombTorizoSamus = getSamus;
        _motherBrainHasAreaBossBit = hasAreaBossBit;
        _hasEvent = hasEvent;
        _setEvent = setEvent;
        _speedBoosterEscapeFx = roomFx;
        _noobTubeRoomFx = roomFx;
        _writeEarthquakeTimer = setEarthquakeTimer;
        _writeNoobTubeEarthquakeType = setEarthquakeType;
        _spawnNoobTubeProjectile = spawnNoobTubeProjectile;
        _downwardGateSamus = getSamus;
        _downwardGateRoomWidth = level.WidthInBlocks;
        _eyeDoorSystem = system;
        _eyeDoorSamus = getSamus;
        _spawnEyeDoorProjectile = spawnEyeDoorProjectile;
        _disableDraygonCannon = disableDraygonCannon;

        ushort populationPointer = population.Pointer;
        int spawnedRecordCount = 0;
        for (int recordIndex = 0; recordIndex < population.Placements.Length; recordIndex++)
        {
            RoomPlmPlacement placement = population.Placements.Span[recordIndex];
            ushort header = placement.Header.Header;
            var record = new RoomPlmPopulationRecord(
                populationPointer, recordIndex,
                unchecked((ushort)(populationPointer + recordIndex * RoomPlmPopulationFormat.RecordByteCount)),
                header, placement.BlockX, placement.BlockY, placement.RoomArgument);

            int blockIndex;
            try
            {
                // `$84:8482-$849F` allocates the ID first and converts the two unsigned
                // coordinates with the room-width multiplier without checking logical
                // dimensions. The bounded native allocation preserves that ordering for
                // the two shipped off-room door records; setup routines that genuinely
                // require authored terrain still validate when they consume the index.
                blockIndex = level.GetPlmBlockIndex(record.BlockX, record.BlockY);
            }
            catch (ArgumentOutOfRangeException error)
            {
                throw new InvalidDataException(
                    $"Room PLM population $8F:{populationPointer:X4} record {recordIndex} " +
                    $"header $84:{header:X4} has block outside safe native allocation " +
                    $"({record.BlockX},{record.BlockY}) and argument ${record.RoomArgument:X4}.",
                    error);
            }

            PlmSlot? slot = AllocateRoomPopulationSlot(
                record, blockIndex, placement.Header.InitialInstruction);
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
                    slot,
                    placement))
            {
                // Do not leave the preallocated but untranslated actor resident. The load is
                // intentionally loud and includes every value needed to locate the exact ROM
                // record without relying on a room-name special case.
                ClearSlot(slot);
                ushort setupPointer = placement.Header.Setup;
                ushort instructionList = placement.Header.InitialInstruction;
                throw new NotSupportedException(
                    $"Room PLM population $8F:{populationPointer:X4} record {recordIndex} " +
                    $"at $8F:{record.RecordPointer:X4} uses untranslated header " +
                    $"$84:{header:X4} (setup $84:{setupPointer:X4}, list " +
                    $"$84:{instructionList:X4}) at block ({record.BlockX},{record.BlockY}) " +
                    $"with argument ${record.RoomArgument:X4}.");
            }

            spawnedRecordCount++;
        }

        return spawnedRecordCount;
    }

    /// <summary>Physical-slot views in cartridge handler order, highest ID first.</summary>
    public IReadOnlyList<RoomPlmSlotSnapshot> PopulationSlots
    {
        get
        {
            var snapshots = new List<RoomPlmSlotSnapshot>(_slots.Length);
            for (int index = _slots.Length - 1; index >= 0; index--)
            {
                PlmSlot slot = _slots[index];
                if (!slot.Active) continue;
                snapshots.Add(new RoomPlmSlotSnapshot(
                    NativeSlotIndex: index,
                    HeaderPointer: slot.HeaderPointer,
                    BlockIndex: slot.BlockIndex,
                    RoomArgument: slot.RoomArgument,
                    InstructionPointer: slot.InstructionPointer,
                    PreInstruction: slot.PreInstruction,
                    InstructionTimer: slot.InstructionTimer,
                    LinkInstruction: slot.LinkInstruction,
                    LoopTimer: slot.LoopTimer));
            }
            return snapshots;
        }
    }

    private PlmSlot? AllocateRoomPopulationSlot(
        RoomPlmPopulationRecord record,
        int blockIndex,
        ushort initialInstruction)
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
            slot.InstructionPointer = initialInstruction;
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
        slot.PlantHeldX = slot.PlantHeldY = 0;
        slot.HeaderPointer = 0;
        slot.BlockIndex = 0;
        slot.RestoreLevelWord = 0;
        slot.InstructionPointer = 0;
        slot.InstructionTimer = 0;
        slot.PreInstruction = 0;
        slot.RoomArgument = 0;
        slot.LoopTimer = 0;
        slot.LinkInstruction = 0;
        slot.Item = null;
        slot.Scroll = null;
        slot.ColoredDoor = null;
        slot.GreyDoor = null;
        slot.Station = null;
        slot.IsElevatorPlatform = false;
        slot.Treadmill = null;
        slot.Gate = null;
        slot.EyeDoor = null;
        slot.DraygonCannon = null;
    }

    private bool TryRunRoomPopulationSetup(
        ISnesAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        SnesVram vram,
        Bank80SystemState system,
        AreaId areaIndex,
        Func<SamusState?> getSamus,
        Func<bool> isAreaTorizoDefeated,
        RoomPlmPopulationRecord record,
        PlmSlot slot,
        RoomPlmPlacement placement)
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

        if (header is RoomPlmHeaders.ScrollTrigger or
            RoomPlmHeaders.RightwardsScrollExtension or
            RoomPlmHeaders.LeftwardsScrollExtension or
            RoomPlmHeaders.DownwardsScrollExtension or
            RoomPlmHeaders.UpwardsScrollExtension)
        {
            SetupScrollSlot(level, slot, header, placement.ScrollProgram.Span, placement.CompiledScrollSource);
            return true;
        }

        if (TryIdentifyPermanentCollectible(
                header,
                out InWorldCollectibleKind kind,
                out CollectiblePresentation presentation))
        {
            SetupCollectibleSlot(level, streamer, vram, system, slot, kind,
                presentation, placement.DynamicGraphic);
            return true;
        }

        if (header == RoomPlmHeaders.MotherBrainGlass)
        {
            SetupMotherBrainGlassSlot(level, streamer, record, slot);
            return true;
        }

        if (header == RoomPlmHeaders.BombTorizoHand)
        {
            SetupBombTorizoHandSlot(level, slot, isAreaTorizoDefeated);
            return true;
        }

        if (header == RoomPlmHeaders.SetMetroidsClearedStatesWhenRequired)
        {
            SetupMetroidsClearedSlot(slot);
            return true;
        }

        if (header == RoomPlmHeaders.SpeedBoosterEscape)
        {
            SetupSpeedBoosterEscapeSlot(slot);
            return true;
        }

        if (header == RoomPlmHeaders.WreckedShipAttic)
        {
            // Setup $84:BAFA deliberately performs no state mutation. Keeping an explicit
            // branch matters: the actor must still consume its native slot and later run
            // its ROM instruction list instead of being mistaken for an unknown header.
            return true;
        }

        if (header == RoomPlmHeaders.NoobTube)
        {
            SetupNoobTubeSlot(level, slot);
            return true;
        }

        if (header == RoomPlmHeaders.MotherBrainEscapeRoomGate)
        {
            SetupDoorTransitionDeactivatedSlot(level, slot);
            return true;
        }

        if (header == RoomPlmHeaders.DownwardGate)
        {
            SetupDownwardGateSlot(level, slot);
            return true;
        }

        if (header == RoomPlmHeaders.DownwardGateShotBlock)
        {
            SetupDownwardGateShotBlock(level, slot);
            return true;
        }

        if (IsEyeDoorHeader(header))
        {
            SetupEyeDoorSlot(level, slot);
            return true;
        }

        if (IsDraygonCannonHeader(header))
        {
            SetupDraygonCannonSlot(level, slot);
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
    ushort InstructionPointer,
    ushort PreInstruction,
    ushort InstructionTimer,
    ushort LinkInstruction,
    ushort LoopTimer);
