using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnCeilingMutationDefinitions;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Spawns Spore Spawn's 2x2 ceiling room object in the room's shared room-object places.</summary>
/// <param name="places">The room's room-object places, which the ceiling contends for with every other room object.</param>
internal sealed partial class SporeSpawnCeilingObjects(RoomObjectPlaceList places)
{
    /// <summary>
    /// Spawns the ceiling room object performing <paramref name="mutation"/> at the ceiling's block. The crumble animates three
    /// crumble frames and then runs on into the clear; the clear starts directly at the clear for an already-defeated room.
    /// </summary>
    public void Spawn(RoomLevelData level, SporeSpawnCeilingMutation mutation)
    {
        Ensure.NotNull(level);
        TerrainChange change = Ensure.Listed(SporeSpawnCeilingMutationDefinitions.Changes, mutation);

        Position16<RoomBlocks> block = level.GridBlock(Block);
        if (places.Spawn() is not { } place)
            return;

        place.Block = block;
        place.Become(new TerrainChangeObject(change));
    }
}
