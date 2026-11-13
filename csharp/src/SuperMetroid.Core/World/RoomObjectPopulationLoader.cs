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
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.World;

/// <summary>Loads a room-object population: installs the room bindings, allocates slots and runs each family setup in record order.</summary>
[RoomLifetime]
internal sealed class RoomObjectPopulationLoader
{
    /// <summary>Handles the Bomb Torizo hand's boss-state setup.</summary>
    private readonly BombTorizoHandObjects _bombTorizoHand;
    /// <summary>Sets up permanent item slots and their dynamic graphics.</summary>
    private readonly CollectibleObjects _collectibleObjects;
    /// <summary>Sets up eye-door objects after allocating their room-object slots.</summary>
    private readonly EyeDoorObjects _eyeDoors;
    /// <summary>Sets up downward gates and their collision changes.</summary>
    private readonly GateObjects _gates;
    /// <summary>Initializes the Mother Brain glass room object.</summary>
    private readonly MotherBrainGlassObjects _motherBrainGlass;
    /// <summary>Descending slot array shared by population and runtime spawns.</summary>
    private readonly RoomObjectSlotTable _slotTable;
    /// <summary>Sets up Maridia's glass tube.</summary>
    private readonly NoobTubeObjects _noobTube;
    /// <summary>Initializes the Speed Booster escape object when its definition appears.</summary>
    private readonly SpeedBoosterEscapeObjects _speedBoosterEscape;

    /// <summary>Connects setup-family services and the shared slot table for one room-object system.</summary>
    internal RoomObjectPopulationLoader(RoomObjectPopulationLoaderDependencies dependencies)
    {
        _bombTorizoHand = dependencies.BombTorizoHand;
        _collectibleObjects = dependencies.CollectibleObjects;
        _eyeDoors = dependencies.EyeDoors;
        _gates = dependencies.Gates;
        _motherBrainGlass = dependencies.MotherBrainGlass;
        _slotTable = dependencies.SlotTable;
        _speedBoosterEscape = dependencies.SpeedBoosterEscape;
        _noobTube = dependencies.NoobTube;
    }
    /// <summary>
    /// Allocates one decoded room population exactly once, in increasing source-record
    /// order. Each record is allocated before its setup runs, as the game's room-object spawn
    /// does. A setup may immediately delete its slot;
    /// the next record can consequently reuse that same highest slot. Retail
    /// loads require decoded records; no population or definition is read from game data.
    /// </summary>
    public int LoadRoomPopulation(RoomObjectRoom room, RoomObjectPopulation population, RoomObjectServices services)
    {
        Ensure.NotNull(room);
        Ensure.NotNull(population);
        Ensure.NotNull(services);
        Ensure.NotNull(services.Samus);
        Ensure.NotNull(services.Bosses);

        int spawnedRecordCount = 0;
        for (int recordIndex = 0; recordIndex < population.Placements.Length; recordIndex++)
        {
            RoomObjectPlacement placement = population.Placements.Span[recordIndex];
            RoomObjectHeaderId header = placement.Header.Header;
            if (DraygonCannonObjects.IsDraygonCannonHeader(header) != placement.Cannon.HasValue)
                throw new InvalidDataException(
                    $"Room-object population {population.Name} record {recordIndex} ({header}) must carry a cannon-control word exactly when it is a Draygon cannon.");
            var record = new RoomObjectPopulationRecord(
                recordIndex, header, placement.Block, placement.Argument);

            Position16<RoomBlocks> block;
            try
            {
                // A room-object spawn allocates the ID first and converts the two unsigned
                // coordinates with the room-width multiplier without checking logical
                // dimensions. The bounded allocation preserves that ordering for
                // the two shipped off-room door records, whose column past the room's width is the block that many
                // columns into the following rows; setups that genuinely require authored terrain still
                // validate when they consume the block.
                block = room.Level.ObjectBlock(record.Block);
            }
            catch (ArgumentOutOfRangeException error)
            {
                throw new InvalidDataException(
                    $"Room-object population {population.Name} record {recordIndex} " +
                    $"header {header} has block outside the room's allocated blocks " +
                    $"({record.Block.X},{record.Block.Y}) and argument {record.Argument}.",
                    error);
            }

            RoomObjectSlot? slot = AllocateRoomPopulationSlot(record, block);
            if (slot is null)
                continue; // No room object spawns while all forty slots are live.

            if (!TryRunRoomPopulationSetup(room, services.Bosses, record, slot, placement))
            {
                // Do not leave the preallocated but untranslated actor resident. The load is
                // intentionally loud and includes every value needed to locate the exact authored
                // record without relying on a room-name special case.
                slot.Delete();
                throw new NotSupportedException(
                    $"Room-object population {population.Name} record {recordIndex} " +
                    $"uses untranslated header " +
                    $"{header} at block ({record.Block.X},{record.Block.Y}) " +
                    $"with argument {record.Argument}.");
            }

            spawnedRecordCount++;
        }

        return spawnedRecordCount;
    }

    /// <summary>Claims the highest free place, which resets whatever it held, and installs the record's identity before setup runs.</summary>
    private RoomObjectSlot? AllocateRoomPopulationSlot(
        RoomObjectPopulationRecord record,
        Position16<RoomBlocks> block)
    {
        if (_slotTable.Spawn() is not { } slot)
            return null;

        slot.Header = record.Header;
        slot.Block = block;
        slot.Argument = record.Argument;
        return slot;
    }

    /// <summary>Dispatches room-population definitions to their family setup; returns false for an unsupported definition so loading can fail with record context.</summary>
    private bool TryRunRoomPopulationSetup(
        RoomObjectRoom room,
        IAreaBossFlagReader bosses,
        RoomObjectPopulationRecord record,
        RoomObjectSlot slot,
        RoomObjectPlacement placement)
    {
        RoomLevelData level = room.Level;
        SharedGameState system = room.System;
        AreaId areaIndex = room.AreaIndex;
        RoomObjectHeaderId header = record.Header;
        if (DoorObjects.TryIdentifyColoredDoor(
                header,
                out ColoredDoorColor color,
                out ColoredDoorOrientation coloredOrientation))
        {
            DoorObjects.SetupColoredDoorSlot(level, slot, color, coloredOrientation);
            return true;
        }

        if (GreyDoorObjects.TryIdentifyGreyDoor(header, out ColoredDoorOrientation greyOrientation))
        {
            GreyDoorObjects.SetupGreyDoorSlot(level, slot,
                new GreyDoorVariant(greyOrientation, BombTorizo: header == RoomObjectHeaderId.BombTorizoGreyDoor));
            return true;
        }

        if (header is RoomObjectHeaderId.ScrollTrigger or
            RoomObjectHeaderId.RightwardsScrollExtension or
            RoomObjectHeaderId.LeftwardsScrollExtension or
            RoomObjectHeaderId.DownwardsScrollExtension or
            RoomObjectHeaderId.UpwardsScrollExtension)
        {
            ScrollObjects.SetupScrollSlot(level, slot, header, placement.ScrollChange, placement.CompiledScrollChange);
            return true;
        }

        if (CollectibleObjects.TryIdentifyPermanentCollectible(
                header,
                out InWorldCollectibleKind kind,
                out CollectiblePresentation presentation))
        {
            _collectibleObjects.SetupCollectibleSlot(room, slot, kind, presentation);
            return true;
        }

        if (header == RoomObjectHeaderId.MotherBrainGlass)
        {
            _motherBrainGlass.SetupMotherBrainGlassSlot(level, record, slot);
            return true;
        }

        if (header == RoomObjectHeaderId.BombTorizoHand)
        {
            _bombTorizoHand.SetupBombTorizoHandSlot(slot, bosses);
            return true;
        }

        if (header == RoomObjectHeaderId.MetroidRoomClearance)
        {
            TourianObjects.SetupMetroidsClearedSlot(slot);
            return true;
        }

        if (header == RoomObjectHeaderId.SpeedBoosterEscape)
        {
            _speedBoosterEscape.SetupSpeedBoosterEscapeSlot(slot);
            return true;
        }

        if (header == RoomObjectHeaderId.WreckedShipAttic)
        {
            // The no-op setup changes nothing. Keeping an explicit branch matters: the object must still hold its
            // place instead of being mistaken for an unknown definition.
            slot.Become(new WreckedShipAtticObject());
            return true;
        }

        if (header == RoomObjectHeaderId.NoobTube)
        {
            _noobTube.SetupNoobTubeSlot(level, slot);
            return true;
        }

        if (header == RoomObjectHeaderId.MotherBrainEscapeRoomGate)
        {
            DoorObjects.SetupDoorTransitionDeactivatedSlot(level, slot);
            slot.Become(new MotherBrainEscapeGateObject(MotherBrainEscapeGateDefinitions.Shut));
            return true;
        }

        if (header == RoomObjectHeaderId.DownwardGate)
        {
            _gates.SetupDownwardGateSlot(level, slot);
            return true;
        }

        if (header == RoomObjectHeaderId.DownwardGateShotBlock)
        {
            GateObjects.SetupDownwardGateShotBlock(level, slot);
            return true;
        }

        if (EyeDoorObjects.IsEyeDoorHeader(header))
        {
            _eyeDoors.SetupEyeDoorSlot(level, slot, header);
            return true;
        }

        if (DraygonCannonObjects.SetupOf(header) is { } cannonSetup)
        {
            DraygonCannonObjects.SetupDraygonCannonSlot(level, slot, cannonSetup, placement.Cannon
                ?? throw new InvalidDataException($"Draygon cannon placement {header} carries no cannon-control word."));
            return true;
        }

        if (StationObjects.TrySetupStationOrElevator(level, system, areaIndex, slot, header))
            return true;

        return false;
    }
}
