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

/// <summary>Loads a room-object population: installs the room bindings, gives each placement a place and runs each family setup in placement order.</summary>
[RoomLifetime]
internal sealed class RoomObjectPopulationLoader
{
    /// <summary>Handles the Bomb Torizo hand's boss-state setup.</summary>
    private readonly BombTorizoHandObjects _bombTorizoHand;
    /// <summary>Sets up permanent items and their dynamic graphics.</summary>
    private readonly CollectibleObjects _collectibleObjects;
    /// <summary>Sets up eye-door objects once they have taken their room-object places.</summary>
    private readonly EyeDoorObjects _eyeDoors;
    /// <summary>Sets up downward gates and their collision changes.</summary>
    private readonly GateObjects _gates;
    /// <summary>Initializes the Mother Brain glass room object.</summary>
    private readonly MotherBrainGlassObjects _motherBrainGlass;
    /// <summary>The room's places, which population and runtime spawns take in update order.</summary>
    private readonly RoomObjectPlaceList _places;
    /// <summary>Sets up Maridia's glass tube.</summary>
    private readonly NoobTubeObjects _noobTube;
    /// <summary>Initializes the Speed Booster escape object when its definition appears.</summary>
    private readonly SpeedBoosterEscapeObjects _speedBoosterEscape;

    /// <summary>Connects setup-family services and the room's places for one room-object system.</summary>
    internal RoomObjectPopulationLoader(RoomObjectPopulationLoaderDependencies dependencies)
    {
        _bombTorizoHand = dependencies.BombTorizoHand;
        _collectibleObjects = dependencies.CollectibleObjects;
        _eyeDoors = dependencies.EyeDoors;
        _gates = dependencies.Gates;
        _motherBrainGlass = dependencies.MotherBrainGlass;
        _places = dependencies.Places;
        _speedBoosterEscape = dependencies.SpeedBoosterEscape;
        _noobTube = dependencies.NoobTube;
    }
    /// <summary>
    /// Places one room-object population exactly once, in its placements' order. Each
    /// placement takes its place before its setup runs, as the game's room-object spawn
    /// does. A setup may immediately delete its place;
    /// the next placement can consequently reuse that same highest place. Loading
    /// requires installed placements; no population or definition is decoded during play.
    /// </summary>
    public void LoadRoomPopulation(RoomObjectRoom room, RoomObjectPopulation population, RoomObjectServices services)
    {
        Ensure.NotNull(room);
        Ensure.NotNull(population);
        Ensure.NotNull(services);
        Ensure.NotNull(services.Samus);
        Ensure.NotNull(services.Bosses);

        for (int placementIndex = 0; placementIndex < population.Placements.Length; placementIndex++)
        {
            RoomObjectPlacement placement = population.Placements.Span[placementIndex];
            RoomObjectKind objectKind = placement.Kind;
            if (DraygonCannonObjects.IsDraygonCannonKind(objectKind) != placement.Cannon.HasValue)
                throw new InvalidDataException(
                    $"Room-object population {population.Name} placement {placementIndex} ({objectKind}) must name the Draygon wall cannon it controls exactly when it is a Draygon cannon.");
            var placed = new PlacedRoomObject(
                objectKind, placement.Block, placement.Argument);

            Position16<RoomBlocks> block;
            try
            {
                // A placement stands on a block of the grid, or, as the two shipped off-room coloured doors do, on a block
                // of the terrain beyond the room (#2034); setups that require the grid's terrain validate when they
                // consume the block.
                block = placement.Site switch
                {
                    RoomObjectSite.RoomGrid => room.Level.GridBlock(placed.Block),
                    RoomObjectSite.BeyondTheRoom => room.Level.BlockBeyondRoom(placed.Block),
                    _ => throw new InvalidDataException($"Undefined {nameof(RoomObjectSite)} {placement.Site}."),
                };
            }
            catch (ArgumentOutOfRangeException error)
            {
                throw new InvalidDataException(
                    $"Room-object population {population.Name} placement {placementIndex} " +
                    $"room object {objectKind} has block ({placed.Block.X},{placed.Block.Y}) outside its {placement.Site} " +
                    $"and argument {placed.Argument}.",
                    error);
            }

            RoomObjectPlace? place = AllocateRoomPopulationPlace(block);
            if (place is null)
                continue; // No room object spawns while all forty places are held.

            if (!TryRunRoomPopulationSetup(room, services.Bosses, placed, place, placement))
            {
                // Do not leave the preallocated but unsupported actor resident. The load is
                // intentionally loud and includes every value needed to locate the exact authored
                // record without relying on a room-name special case.
                place.Delete();
                throw new NotSupportedException(
                    $"Room-object population {population.Name} placement {placementIndex} " +
                    $"uses unsupported room-object kind " +
                    $"{objectKind} at block ({placed.Block.X},{placed.Block.Y}) " +
                    $"with argument {placed.Argument}.");
            }
        }
    }

    /// <summary>Claims the highest free place, which resets whatever it held, and installs the record's block before setup runs.</summary>
    private RoomObjectPlace? AllocateRoomPopulationPlace(Position16<RoomBlocks> block)
    {
        if (_places.Spawn() is not { } place)
            return null;

        place.Block = block;
        return place;
    }

    /// <summary>
    /// Hands room-population definitions to their family setup, with what the placement gave the object (#2091); returns
    /// false for an unsupported definition so loading can fail with record context.
    /// </summary>
    private bool TryRunRoomPopulationSetup(
        RoomObjectRoom room,
        IAreaBossFlagReader bosses,
        PlacedRoomObject placed,
        RoomObjectPlace place,
        RoomObjectPlacement placement)
    {
        RoomLevelData level = room.Level;
        SharedGameState system = room.System;
        AreaId area = room.Area;
        RoomObjectKind objectKind = placed.Kind;
        if (DoorObjects.TryIdentifyColoredDoor(
                objectKind,
                out ColoredDoorColor color,
                out ColoredDoorOrientation coloredOrientation))
        {
            DoorObjects.SetupColoredDoor(level, place, color, coloredOrientation, placed.Argument as LockedDoorArgument);
            return true;
        }

        if (GreyDoorObjects.TryIdentifyGreyDoor(objectKind, out ColoredDoorOrientation greyOrientation))
        {
            GreyDoorObjects.SetupGreyDoor(level, place,
                new GreyDoorVariant(greyOrientation, BombTorizo: objectKind == RoomObjectKind.BombTorizoGreyDoor),
                placed.Argument as GreyDoorArgument);
            return true;
        }

        if (objectKind is RoomObjectKind.ScrollTrigger or
            RoomObjectKind.RightwardsScrollExtension or
            RoomObjectKind.LeftwardsScrollExtension or
            RoomObjectKind.DownwardsScrollExtension or
            RoomObjectKind.UpwardsScrollExtension)
        {
            ScrollObjects.SetupScroll(level, place, objectKind, placement.ScrollChange, placement.CompiledScrollChange);
            return true;
        }

        if (CollectibleObjects.TryIdentifyPermanentCollectible(
                objectKind,
                out InWorldCollectibleKind kind,
                out CollectiblePresentation presentation))
        {
            _collectibleObjects.SetupCollectible(room, place, kind, presentation, placed.Argument as CollectedItemArgument);
            return true;
        }

        if (objectKind == RoomObjectKind.MotherBrainGlass)
        {
            _motherBrainGlass.SetupMotherBrainGlass(level, place);
            return true;
        }

        if (objectKind == RoomObjectKind.BombTorizoHand)
        {
            _bombTorizoHand.SetupBombTorizoHand(place, bosses);
            return true;
        }

        if (objectKind == RoomObjectKind.MetroidRoomClearance)
        {
            TourianObjects.SetupMetroidsCleared(place, placed.Argument as MetroidRoomArgument);
            return true;
        }

        if (objectKind == RoomObjectKind.SpeedBoosterEscape)
        {
            _speedBoosterEscape.SetupSpeedBoosterEscape(place);
            return true;
        }

        if (objectKind == RoomObjectKind.WreckedShipAttic)
        {
            // The no-op setup changes nothing. Keeping an explicit branch matters: the object must still hold its
            // place instead of being mistaken for an unknown definition.
            place.Become(new WreckedShipAtticObject());
            return true;
        }

        if (objectKind == RoomObjectKind.NoobTube)
        {
            _noobTube.SetupNoobTube(level, place);
            return true;
        }

        if (objectKind == RoomObjectKind.MotherBrainEscapeRoomGate)
        {
            DoorObjects.SetupPassableBlock(level, place);
            place.Become(new MotherBrainEscapeGateObject(MotherBrainEscapeGateDefinitions.Shut));
            return true;
        }

        if (objectKind == RoomObjectKind.DownwardGate)
        {
            _gates.SetupDownwardGate(level, place);
            return true;
        }

        if (objectKind == RoomObjectKind.DownwardGateShotBlock)
        {
            GateObjects.SetupDownwardGateShotBlock(level, place, placed.Argument as GateTriggerArgument);
            return true;
        }

        if (EyeDoorObjects.IsEyeDoorKind(objectKind))
        {
            _eyeDoors.SetupEyeDoor(level, place, objectKind, placed.Argument as LockedDoorArgument);
            return true;
        }

        if (DraygonCannonObjects.SetupOf(objectKind) is { } cannonSetup)
        {
            DraygonCannonObjects.SetupDraygonCannon(level, place, cannonSetup, placement.Cannon
                ?? throw new InvalidDataException($"Draygon cannon placement {objectKind} names no Draygon wall cannon."));
            return true;
        }

        if (StationObjects.TrySetupStationOrElevator(level, system, area, place, objectKind, placed.Argument as SaveStationArgument))
            return true;

        return false;
    }
}
