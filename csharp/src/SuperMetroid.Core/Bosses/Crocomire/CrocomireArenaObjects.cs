using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Core.Bosses.Crocomire.CrocomireArenaDraws;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Spawns Crocomire's bridge and invisible-wall room objects in the room's shared room-object places.</summary>
/// <param name="places">The room's room-object places, which the arena mutations contend for with every other room object.</param>
internal sealed partial class CrocomireArenaObjects(RoomObjectPlaceList places)
{
    /// <summary>
    /// Spawns one of Crocomire's arena mutations at its block. Each mutation's setup does nothing; its terrain change
    /// draws once and is gone on the next update.
    /// </summary>
    public void Spawn(RoomLevelData level, RoomObjectSpawnRequest<CrocomireArenaMutation> request)
    {
        Ensure.NotNull(level);
        TerrainChange change = Changes[request.RoomObject];

        Position16<RoomBlocks> block = level.GridBlock(request.Block);
        if (places.Spawn() is not { } place)
            return;

        place.Block = block;
        place.Become(new TerrainChangeObject(change));
    }
}
