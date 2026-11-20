using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Core.Bosses.Botwoon.BotwoonWallMutationDefinitions;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Spawns Botwoon's wall room object in the room's shared room-object places.</summary>
/// <param name="places">The room's room-object places, which the wall contends for with every other room object.</param>
internal sealed class BotwoonWallObjects(RoomObjectPlaceList places)
{
    /// <summary>
    /// Spawns the wall room object performing <paramref name="mutation"/> at the wall's top block, in the first free place of the
    /// room's processing order. The wall is known by what it does (its terrain change, or
    /// <see cref="BotwoonWallCrumbleObject"/>), never by a room-object identity.
    /// </summary>
    /// <remarks>
    /// The clear has an empty setup and clears an already-defeated room on the next update. The crumble's setup holds
    /// it for its setup hold (<see cref="BotwoonWallMutationDefinitions.CrumbleSetupHold"/>); it then crumbles the wall's rows
    /// through their images, moving one block down between rows.
    /// </remarks>
    public void Spawn(RoomLevelData level, BotwoonWallMutation mutation)
    {
        Ensure.NotNull(level);
        RoomObjectBehaviour wall = mutation switch
        {
            BotwoonWallMutation.Clear => new TerrainChangeObject(BotwoonWallMutationDefinitions.Clear),
            BotwoonWallMutation.Crumble => new BotwoonWallCrumbleObject(),
            _ => throw new ArgumentOutOfRangeException(nameof(mutation), mutation, null),
        };

        // Resolve the block through the level so a malformed room cannot install a wall whose later vertical draws
        // fall outside the room.
        Position16<RoomBlocks> block = level.GridBlock(Block);
        if (places.Spawn() is not { } place)
            return;

        place.Block = block;
        place.Become(wall);
    }
}
