using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Core.Bosses.Botwoon.BotwoonWallMutationDefinitions;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Spawns Botwoon's wall PLM in the room's shared PLM slots.</summary>
/// <param name="slotTable">The room's PLM slots, which the wall contends for with every other room PLM.</param>
internal sealed class BotwoonWallObjects(RoomObjectSlotTable slotTable)
{
    /// <summary>
    /// Spawns the wall PLM performing <paramref name="mutation"/> at the wall's top block, in the first free slot of the
    /// room's processing order.
    /// </summary>
    /// <remarks>
    /// The clear has an empty setup and clears an already-defeated room on the next update. The crumble's setup holds
    /// it for 64 updates; it then crumbles nine rows of four four-update images, moving one block down between rows.
    /// </remarks>
    /// <returns>False only when every PLM slot is occupied; nothing is spawned then.</returns>
    public bool TrySpawn(RoomLevelData level, BotwoonWallMutation mutation)
    {
        Ensure.NotNull(level);
        RoomObjectHeaderId roomObject = Ensure.Listed(RoomObjects, mutation);

        // Resolve the block through the level so a malformed fixture cannot install a wall whose later vertical draws
        // fall outside the room.
        int blockIndex = level.GetBlockIndex(Block);
        if (slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.BlockIndex = blockIndex;
        // The place keeps the wall's identity for owner queries and mixed populations.
        slot.Header = roomObject;
        slot.Become(mutation switch
        {
            BotwoonWallMutation.Clear => new TerrainChangeObject(BotwoonWallMutationDefinitions.Clear),
            BotwoonWallMutation.Crumble => new BotwoonWallCrumbleObject(),
            _ => throw new ArgumentOutOfRangeException(nameof(mutation), mutation, null),
        });
        return true;
    }
}
