using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.World;

/// <summary>Loads a room-object population: installs the room bindings, gives each placement a place and runs each family setup in placement order.</summary>
[RoomLifetime]
internal sealed partial class RoomObjectPopulationLoader
{
    /// <summary>The room-object families whose setups keep state of their own, and the room's places.</summary>
    private readonly RoomObjectPopulationLoaderDependencies _families;

    /// <summary>The room's enemy projectiles' spawners, which the room objects that throw projectiles take as they set up.</summary>
    private readonly RoomObjectThrownProjectileSpawners _thrown;

    /// <summary>
    /// Connects setup-family services and the room's places for one room-object system, and the room's enemy projectiles'
    /// spawners of the projectiles its room objects throw.
    /// </summary>
    internal RoomObjectPopulationLoader(RoomObjectPopulationLoaderDependencies dependencies, RoomObjectThrownProjectileSpawners thrown)
    {
        _families = Ensure.NotNull(dependencies);
        _thrown = Ensure.NotNull(thrown);
    }

    /// <summary>
    /// Places one room-object population exactly once, in its placements' order. Each
    /// placement takes its place before its setup runs, as the game's room-object spawn
    /// does. A setup may immediately delete its place;
    /// the next placement can consequently reuse that same place. Loading
    /// requires installed placements; no population or definition is decoded during play. Once every setup has run, the
    /// room's scroll-trigger blocks are bound to their triggers.
    /// </summary>
    /// <exception cref="InvalidDataException">A placement lies outside its site, or a scroll-trigger block has no
    /// trigger.</exception>
    public void LoadRoomPopulation(RoomObjectRoom room, RoomObjectPopulation population, RoomObjectServices services)
    {
        Ensure.NotNull(room);
        Ensure.NotNull(population);
        Ensure.NotNull(services);

        for (int placementIndex = 0; placementIndex < population.Placements.Length; placementIndex++)
        {
            RoomObjectPlacement placement = population.Placements.Span[placementIndex];
            RoomObjectKind objectKind = placement.Kind;

            Position16<RoomBlocks> block;
            try
            {
                // A placement stands on a block of the grid, or, as the two shipped off-room coloured doors do, on a block
                // off it (#2034); setups that require the grid's terrain validate when they consume the block.
                block = placement.Site == RoomObjectSite.RoomGrid
                    ? room.Level.GridBlock(placement.Block)
                    : room.Level.BlockOffTheGrid(placement.Block);
            }
            catch (ArgumentOutOfRangeException error)
            {
                throw new InvalidDataException(
                    $"Room-object population {population.Name} placement {placementIndex} " +
                    $"room object {objectKind} has block ({placement.Block.X},{placement.Block.Y}) outside its {placement.Site} " +
                    $"and setup {placement.Setup}.",
                    error);
            }

            RoomObjectPlace? place = AllocateRoomPopulationPlace(block);
            if (place is null)
                continue; // No room object spawns while every place is held.

            if (placement.Site == RoomObjectSite.OffTheRoom)
            {
                // Nothing in the room reaches a coloured door off its grid, the one setup placed there: all it does is hold
                // its place (#2572).
                place.Become(new OffRoomDoorHeldPlaceObject());
                continue;
            }

            // The placement carries its family's setup, which runs with what the placement gave the object (#2091, #2288, #2383).
            placement.Setup.SetUp(new RoomObjectFamilySetups(new RoomObjectSetupContext
            {
                Room = room,
                Bosses = services.Bosses,
                Place = place,
                Families = _families,
                Thrown = _thrown,
            }));
        }

        // Every placement's setup has written its blocks; each scroll-trigger block is now bound to its trigger.
        _families.Scrolls.BindTriggerBlocks(room.Level);
    }

    /// <summary>Takes the first free place in update order, which starts afresh, and gives it the placement's block
    /// before its setup runs.</summary>
    private RoomObjectPlace? AllocateRoomPopulationPlace(Position16<RoomBlocks> block)
    {
        if (_families.Places.Spawn() is not { } place)
            return null;

        place.Block = block;
        return place;
    }
}
