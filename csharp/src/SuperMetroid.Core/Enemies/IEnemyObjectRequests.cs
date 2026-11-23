using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies;

/// <summary>The room-object work the room's room objects hand its enemies each frame.</summary>
internal interface IEnemyObjectRequests
{
    /// <summary>Spawns or wakes the downward gate's projectile a gate room object requested.</summary>
    void ApplyDownwardGateProjectileRequest(DownwardGateProjectileRequest request);
}
