using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.World;

/// <summary>Loads a room PLM population: installs the room bindings, allocates native slots and runs each family setup in record order.</summary>
[RoomLifetime]
internal sealed class PlmPopulationLoader
{
    /// <summary>Handles the Bomb Torizo hand's boss-state setup.</summary>
    private readonly BombTorizoHandPlms _bombTorizoHand;
    /// <summary>Sets up permanent item slots and their dynamic graphics.</summary>
    private readonly CollectiblePlms _collectiblePlms;
    /// <summary>Sets up eye-door headers after allocating their native PLM slots.</summary>
    private readonly EyeDoorPlms _eyeDoors;
    /// <summary>Sets up downward gates and their collision changes.</summary>
    private readonly GatePlms _gates;
    /// <summary>Initializes the Mother Brain glass room object.</summary>
    private readonly MotherBrainGlassPlms _motherBrainGlass;
    /// <summary>Native descending slot array shared by population and runtime spawns.</summary>
    private readonly PlmSlotTable _slotTable;
    /// <summary>Initializes the Speed Booster escape object when its header appears.</summary>
    private readonly SpeedBoosterEscapePlms _speedBoosterEscape;

    /// <summary>Connects setup-family services and the shared slot table for one PLM system.</summary>
    internal PlmPopulationLoader(PlmPopulationLoaderDependencies dependencies)
    {
        _bombTorizoHand = dependencies.BombTorizoHand;
        _collectiblePlms = dependencies.CollectiblePlms;
        _eyeDoors = dependencies.EyeDoors;
        _gates = dependencies.Gates;
        _motherBrainGlass = dependencies.MotherBrainGlass;
        _slotTable = dependencies.SlotTable;
        _speedBoosterEscape = dependencies.SpeedBoosterEscape;
    }
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

        int spawnedRecordCount = 0;
        for (int recordIndex = 0; recordIndex < population.Placements.Length; recordIndex++)
        {
            RoomPlmPlacement placement = population.Placements.Span[recordIndex];
            PlmHeaderId header = placement.Header.Header;
            var record = new RoomPlmPopulationRecord(
                recordIndex, header, placement.Block, placement.RoomArgument);

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
                    $"Room PLM population {population.Name} record {recordIndex} " +
                    $"header $84:{(int)header:X4} has block outside safe native allocation " +
                    $"({record.Block.X},{record.Block.Y}) and argument ${record.RoomArgument:X4}.",
                    error);
            }

            PlmSlot? slot = AllocateRoomPopulationSlot(record, blockIndex);
            if (slot is null)
                continue; // The native routine returns carry set when all forty IDs are live.

            if (!TryRunRoomPopulationSetup(room, services.Bosses, record, slot, placement))
            {
                // Do not leave the preallocated but untranslated actor resident. The load is
                // intentionally loud and includes every value needed to locate the exact ROM
                // record without relying on a room-name special case.
                slot.Clear();
                ushort setupPointer = placement.Header.Setup;
                ushort instructionList = placement.Header.InitialInstruction;
                throw new NotSupportedException(
                    $"Room PLM population {population.Name} record {recordIndex} " +
                    $"uses untranslated header " +
                    $"$84:{(int)header:X4} (setup $84:{setupPointer:X4}, list " +
                    $"$84:{instructionList:X4}) at block ({record.Block.X},{record.Block.Y}) " +
                    $"with argument ${record.RoomArgument:X4}.");
            }

            spawnedRecordCount++;
        }

        return spawnedRecordCount;
    }

    /// <summary>Claims the highest inactive native slot, resets recycled family state, and installs record identity before setup runs.</summary>
    private PlmSlot? AllocateRoomPopulationSlot(
        RoomPlmPopulationRecord record,
        int blockIndex)
    {
        if (_slotTable.SpawnRoomPlm() is not { } slot)
            return null;

        slot.HeaderPointer = record.HeaderPointer;
        slot.BlockIndex = blockIndex;
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.RoomArgument = record.RoomArgument;
        return slot;
    }

    /// <summary>Dispatches translated room-population headers to their family setup; returns false for an unsupported header so loading can fail with record context.</summary>
    private bool TryRunRoomPopulationSetup(
        PlmRoom room,
        IAreaBossFlagReader bosses,
        RoomPlmPopulationRecord record,
        PlmSlot slot,
        RoomPlmPlacement placement)
    {
        RoomLevelData level = room.Level;
        Bank80SystemState system = room.System;
        AreaId areaIndex = room.AreaIndex;
        PlmHeaderId header = record.HeaderPointer;
        if (DoorPlms.TryIdentifyColoredDoor(
                header,
                out ColoredDoorColor color,
                out ColoredDoorOrientation coloredOrientation))
        {
            DoorPlms.SetupColoredDoorSlot(level, slot, color, coloredOrientation);
            return true;
        }

        if (GreyDoorPlms.TryIdentifyGreyDoor(header, out ColoredDoorOrientation greyOrientation))
        {
            GreyDoorPlms.SetupGreyDoorSlot(level, slot, greyOrientation);
            return true;
        }

        if (header is PlmHeaderId.ScrollTrigger or
            PlmHeaderId.RightwardsScrollExtension or
            PlmHeaderId.LeftwardsScrollExtension or
            PlmHeaderId.DownwardsScrollExtension or
            PlmHeaderId.UpwardsScrollExtension)
        {
            ScrollPlms.SetupScrollSlot(level, slot, header, placement.ScrollProgram.Span, placement.CompiledScrollSource);
            return true;
        }

        if (CollectiblePlms.TryIdentifyPermanentCollectible(
                header,
                out InWorldCollectibleKind kind,
                out CollectiblePresentation presentation))
        {
            _collectiblePlms.SetupCollectibleSlot(room, slot, kind,
                presentation, placement.DynamicGraphic);
            return true;
        }

        if (header == PlmHeaderId.MotherBrainGlass)
        {
            _motherBrainGlass.SetupMotherBrainGlassSlot(level, record, slot);
            return true;
        }

        if (header == PlmHeaderId.BombTorizoHand)
        {
            _bombTorizoHand.SetupBombTorizoHandSlot(level, slot, bosses);
            return true;
        }

        if (header == PlmHeaderId.SetMetroidsClearedStatesWhenRequired)
        {
            TourianPlms.SetupMetroidsClearedSlot(slot);
            return true;
        }

        if (header == PlmHeaderId.SpeedBoosterEscape)
        {
            _speedBoosterEscape.SetupSpeedBoosterEscapeSlot(slot);
            return true;
        }

        if (header == PlmHeaderId.WreckedShipAttic)
        {
            // Setup $84:BAFA deliberately performs no state mutation. Keeping an explicit
            // branch matters: the actor must still consume its native slot and run its
            // program instead of being mistaken for an unknown header.
            slot.StartAnimation(new WreckedShipAtticPlmAnimation());
            return true;
        }

        if (header == PlmHeaderId.NoobTube)
        {
            NoobTubePlms.SetupNoobTubeSlot(level, slot);
            return true;
        }

        if (header == PlmHeaderId.MotherBrainEscapeRoomGate)
        {
            DoorPlms.SetupDoorTransitionDeactivatedSlot(level, slot);
            slot.StartAnimation(new ScriptedRoomPlmAnimation(ScriptedRoomPlmProgram.MotherBrainEscapeGate));
            return true;
        }

        if (header == PlmHeaderId.DownwardGate)
        {
            _gates.SetupDownwardGateSlot(level, slot);
            return true;
        }

        if (header == PlmHeaderId.DownwardGateShotBlock)
        {
            GatePlms.SetupDownwardGateShotBlock(level, slot);
            return true;
        }

        if (EyeDoorPlms.IsEyeDoorHeader(header))
        {
            _eyeDoors.SetupEyeDoorSlot(level, slot);
            return true;
        }

        if (DraygonCannonPlms.IsDraygonCannonHeader(header))
        {
            DraygonCannonPlms.SetupDraygonCannonSlot(level, slot);
            return true;
        }

        if (StationPlms.TrySetupStationOrElevator(level, system, areaIndex, slot))
            return true;

        return false;
    }
}
