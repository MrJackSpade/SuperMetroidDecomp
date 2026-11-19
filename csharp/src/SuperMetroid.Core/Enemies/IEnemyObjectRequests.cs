using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies;

/// <summary>The room-object work enemies and their room objects hand each other each frame.</summary>
internal interface IEnemyObjectRequests
{
    /// <summary>
    /// Starts the Chozo statue's hand sequence once Samus has triggered its hand, returning the room objects the trigger
    /// asks for, which the room spawns at once.
    /// </summary>
    IReadOnlyList<RoomObjectSpawnRequest<ChozoStatueRoomObject>> ActivateChozoStatueHandTrigger();

    /// <summary>Spawns or wakes the downward gate's projectile a gate room object requested.</summary>
    void ApplyDownwardGateProjectileRequest(DownwardGateProjectileRequest request);
}
