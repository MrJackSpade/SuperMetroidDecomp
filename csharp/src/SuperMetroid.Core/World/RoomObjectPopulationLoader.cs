using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.World;

/// <summary>Loads a room-object population: installs the room bindings, gives each placement a place and runs each family setup in placement order.</summary>
[RoomLifetime]
internal sealed class RoomObjectPopulationLoader
{
    /// <summary>The room-object families whose setups keep state of their own, and the room's places.</summary>
    private readonly RoomObjectPopulationLoaderDependencies _families;

    /// <summary>Connects setup-family services and the room's places for one room-object system.</summary>
    internal RoomObjectPopulationLoader(RoomObjectPopulationLoaderDependencies dependencies) =>
        _families = Ensure.NotNull(dependencies);
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
            RoomObjectSetup setup = RoomObjectSetups.Of(objectKind);
            if (setup is RoomObjectSetup.DraygonCannon != placement.Cannon.HasValue)
                throw new InvalidDataException(
                    $"Room-object population {population.Name} placement {placementIndex} ({objectKind}) must name the Draygon wall cannon it controls exactly when it is a Draygon cannon.");
            var placed = new PlacedRoomObject(
placement.Block, placement.Argument);

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

            // The placement's kind names its family's setup, which runs with what the placement gave the object (#2091, #2288).
            setup.SetUp(new RoomObjectSetupContext
            {
                Room = room,
                Bosses = services.Bosses,
                Place = place,
                Placement = placement,
                Families = _families,
            });
        }
    }

    /// <summary>Claims the highest free place, which resets whatever it held, and installs the record's block before setup runs.</summary>
    private RoomObjectPlace? AllocateRoomPopulationPlace(Position16<RoomBlocks> block)
    {
        if (_families.Places.Spawn() is not { } place)
            return null;

        place.Block = block;
        return place;
    }
}
