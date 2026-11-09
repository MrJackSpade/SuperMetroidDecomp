using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies;

/// <summary>The room-object work enemies and their room objects hand each other each frame.</summary>
internal interface IEnemyObjectRequests
{
    /// <summary>Applies the Chozo statue room-object work the statues requested this frame.</summary>
    void ApplyPendingChozoStatueObjects(RoomLevelData level, ChozoStatueObjects chozoStatueObjects);

    /// <summary>Starts the Chozo statue's hand sequence from the hand block Samus landed on.</summary>
    void ActivateChozoStatueHandTrigger(RoomLevelData level, int collisionBlockIndex);

    /// <summary>Spawns or wakes the downward gate's projectile a gate room object requested.</summary>
    void ApplyDownwardGateProjectileRequest(DownwardGateProjectileRequest request, int roomWidthInBlocks);
}
