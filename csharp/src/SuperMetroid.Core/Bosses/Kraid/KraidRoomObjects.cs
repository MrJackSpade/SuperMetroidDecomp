using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Core.Bosses.Kraid.KraidRoomMutationDefinitions;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Spawns Kraid's ceiling, platform and spike room objects in the room's shared room-object places.</summary>
/// <param name="places">The room's room-object places, which Kraid's mutations contend for with every other room object.</param>
internal sealed partial class KraidRoomObjects(RoomObjectPlaceList places)
{
    /// <summary>
    /// Spawns one of Kraid's room mutations at its block and runs its deactivating setup. The setup immediately
    /// deactivates the block's collision while keeping its appearance; the mutation's terrain change then draws its
    /// crumble frames on the authored cadence and is gone.
    /// </summary>
    public void Spawn(RoomLevelData level, RoomObjectSpawnRequest<KraidRoomMutation> request)
    {
        Ensure.NotNull(level);
        TerrainChange change = Ensure.Listed(Changes, request.RoomObject);

        Position16<RoomBlocks> block = level.GridBlock(request.Block);
        if (places.Spawn() is not { } place)
            return;

        place.Block = block;
        place.Become(new TerrainChangeObject(change));

        // The deactivating setup keeps a solid block solid and turns every other block to air, keeping what it shows (#2502).
        level.Deactivate(block);
    }
}
