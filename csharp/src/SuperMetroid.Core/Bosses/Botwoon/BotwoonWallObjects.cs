using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Core.Bosses.Botwoon.BotwoonWallMutationDefinitions;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Spawns Botwoon's wall room object in the room's shared room-object slots.</summary>
/// <param name="slotTable">The room's room-object slots, which the wall contends for with every other room object.</param>
internal sealed class BotwoonWallObjects(RoomObjectSlotTable slotTable)
{
    /// <summary>
    /// Spawns the wall room object performing <paramref name="mutation"/> at the wall's top block, in the first free slot of the
    /// room's processing order. The wall is known by what it does (its terrain change, or
    /// <see cref="BotwoonWallCrumbleObject"/>), never by a room-object identity.
    /// </summary>
    /// <remarks>
    /// The clear has an empty setup and clears an already-defeated room on the next update. The crumble's setup holds
    /// it for 64 updates; it then crumbles nine rows of four four-update images, moving one block down between rows.
    /// </remarks>
    /// <returns>
    /// False when the room is full; nothing is spawned then (<see cref="Compatibility.RoomObjectPlaces"/>).
    /// </returns>
    public bool TrySpawn(RoomLevelData level, BotwoonWallMutation mutation)
    {
        Ensure.NotNull(level);
        RoomObjectBehaviour wall = mutation switch
        {
            BotwoonWallMutation.Clear => new TerrainChangeObject(BotwoonWallMutationDefinitions.Clear),
            BotwoonWallMutation.Crumble => new BotwoonWallCrumbleObject(),
            _ => throw new ArgumentOutOfRangeException(nameof(mutation), mutation, null),
        };

        // Resolve the block through the level so a malformed fixture cannot install a wall whose later vertical draws
        // fall outside the room.
        Position16<RoomBlocks> block = level.GridBlock(Block);
        if (slotTable.Spawn() is not { } slot)
            return false;

        slot.Block = block;
        slot.Become(wall);
        return true;
    }
}
