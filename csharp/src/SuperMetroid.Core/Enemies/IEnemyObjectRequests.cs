using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies;

/// <summary>The room-object work enemies and their room objects hand each other each frame.</summary>
internal interface IEnemyObjectRequests
{
    /// <summary>Spawns or wakes the downward gate's projectile a gate room object requested.</summary>
    void ApplyDownwardGateProjectileRequest(DownwardGateProjectileRequest request);
}
