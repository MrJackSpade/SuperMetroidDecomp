using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Cartridge-authentic construction of bank-$84 room PLMs from compiled retail
/// bank-$8F placements or an explicitly supplied synthetic population.
/// </summary>
public sealed partial class RoomPlmSystem
{
    /// <summary>
    /// Allocates one decoded room population exactly once, in increasing source-record
    /// order. Each record is allocated before its setup routine runs, matching
    /// <c>Spawn_Room_PLM</c> at <c>$84:846A</c>. A setup may immediately delete its slot;
    /// the next record can consequently reuse that same highest native slot. Retail
    /// loads require decoded records; no population or header can be read from a bus.
    /// </summary>
    public int LoadRoomPopulation(PlmRoom room, RoomPlmPopulationDefinition population, RoomPlmServices services)
    {
        Ensure.NotNull(room);
        Ensure.NotNull(population);
        Ensure.NotNull(services);
        Ensure.NotNull(services.Samus);
        Ensure.NotNull(services.Bosses);

        // These are room-owned services used later by resident pre-instructions. They are
        // installed once, not once per family scan, so every actor in this population sees
        // the same cartridge/runtime state owner.
        _coloredDoorSystem = room.System;
        _greyDoorSystem = room.System;
        _greyDoorArea = room.AreaIndex;
        _activeAreaIndex = room.AreaIndex;
        _tourianStatue = services.TourianStatue;
        _collectibleSystem = room.System;
        _collectibleSamus = services.Samus;
        _bombTorizoSamus = services.Samus;
        _motherBrainBosses = services.Bosses;
        _events = services.Events;
        _speedBoosterEscapeFx = services.RoomFx;
        _noobTubeRoomFx = services.RoomFx;
        _roomEarthquake = services.Earthquake;
        _noobTubeEarthquake = services.Earthquake;
        _noobTubeEnemies = services.Enemies;
        _downwardGateSamus = services.Samus;
        _downwardGateRoomWidth = room.Level.Size.X;
        _eyeDoorSystem = room.System;
        _eyeDoorSamus = services.Samus;
        _eyeDoorEnemies = services.Enemies;
        _oldTourianWallEnemies = services.Enemies;
        _draygonCannonEnemies = services.Enemies;

        ushort populationPointer = population.Pointer;
        int spawnedRecordCount = 0;
        for (int recordIndex = 0; recordIndex < population.Placements.Length; recordIndex++)
        {
            RoomPlmPlacement placement = population.Placements.Span[recordIndex];
            PlmHeaderId header = placement.Header.Header;
            var record = new RoomPlmPopulationRecord(
recordIndex,
                unchecked((ushort)(populationPointer + recordIndex * RoomPlmPopulationFormat.RecordByteCount)),
                header, placement.Block, placement.RoomArgument);

            int blockIndex;
            try
            {
                // `$84:8482-$849F` allocates the ID first and converts the two unsigned
                // coordinates with the room-width multiplier without checking logical
                // dimensions. The bounded native allocation preserves that ordering for
                // the two shipped off-room door records; setup routines that genuinely
                // require authored terrain still validate when they consume the index.
                blockIndex = room.Level.GetPlmBlockIndex(record.Block);
            }
            catch (ArgumentOutOfRangeException error)
            {
                throw new InvalidDataException(
                    $"Room PLM population $8F:{populationPointer:X4} record {recordIndex} " +
                    $"header $84:{(int)header:X4} has block outside safe native allocation " +
                    $"({record.Block.X},{record.Block.Y}) and argument ${record.RoomArgument:X4}.",
                    error);
            }

            PlmSlot? slot = AllocateRoomPopulationSlot(
                record, blockIndex, placement.Header.InitialInstruction);
            if (slot is null)
                continue; // The native routine returns carry set when all forty IDs are live.

            if (!TryRunRoomPopulationSetup(room, services.Bosses, record, slot, placement))
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
                    $"$84:{(int)header:X4} (setup $84:{setupPointer:X4}, list " +
                    $"$84:{instructionList:X4}) at block ({record.Block.X},{record.Block.Y}) " +
                    $"with argument ${record.RoomArgument:X4}.");
            }

            spawnedRecordCount++;
        }

        return spawnedRecordCount;
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
        slot.PlantHeldPosition = Position16<RoomPixels>.Zero;
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
        PlmRoom room,
        IAreaBossFlagReader bosses,
        RoomPlmPopulationRecord record,
        PlmSlot slot,
        RoomPlmPlacement placement)
    {
        (ISnesAddressSpace bus, RoomLevelData level, BackgroundTilemapStreamer streamer, SnesVram vram,
            Bank80SystemState system, AreaId areaIndex) =
            (room.Bus, room.Level, room.Streamer, room.Vram, room.System, room.AreaIndex);
        PlmHeaderId header = record.HeaderPointer;
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

        if (header is PlmHeaderId.ScrollTrigger or
            PlmHeaderId.RightwardsScrollExtension or
            PlmHeaderId.LeftwardsScrollExtension or
            PlmHeaderId.DownwardsScrollExtension or
            PlmHeaderId.UpwardsScrollExtension)
        {
            SetupScrollSlot(level, slot, header, placement.ScrollProgram.Span, placement.CompiledScrollSource);
            return true;
        }

        if (TryIdentifyPermanentCollectible(
                header,
                out InWorldCollectibleKind kind,
                out CollectiblePresentation presentation))
        {
            SetupCollectibleSlot(room, slot, kind,
                presentation, placement.DynamicGraphic);
            return true;
        }

        if (header == PlmHeaderId.MotherBrainGlass)
        {
            SetupMotherBrainGlassSlot(level, streamer, record, slot);
            return true;
        }

        if (header == PlmHeaderId.BombTorizoHand)
        {
            SetupBombTorizoHandSlot(level, slot, bosses);
            return true;
        }

        if (header == PlmHeaderId.SetMetroidsClearedStatesWhenRequired)
        {
            SetupMetroidsClearedSlot(slot);
            return true;
        }

        if (header == PlmHeaderId.SpeedBoosterEscape)
        {
            SetupSpeedBoosterEscapeSlot(slot);
            return true;
        }

        if (header == PlmHeaderId.WreckedShipAttic)
        {
            // Setup $84:BAFA deliberately performs no state mutation. Keeping an explicit
            // branch matters: the actor must still consume its native slot and later run
            // its ROM instruction list instead of being mistaken for an unknown header.
            return true;
        }

        if (header == PlmHeaderId.NoobTube)
        {
            SetupNoobTubeSlot(level, slot);
            return true;
        }

        if (header == PlmHeaderId.MotherBrainEscapeRoomGate)
        {
            SetupDoorTransitionDeactivatedSlot(level, slot);
            return true;
        }

        if (header == PlmHeaderId.DownwardGate)
        {
            SetupDownwardGateSlot(level, slot);
            return true;
        }

        if (header == PlmHeaderId.DownwardGateShotBlock)
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
    int RecordIndex,
    ushort RecordPointer,
    PlmHeaderId HeaderPointer,
    Position16<RoomBlocks> Block,
    ushort RoomArgument);
