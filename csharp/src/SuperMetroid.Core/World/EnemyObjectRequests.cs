using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.World;

/// <summary>The PLM work over the Chozo statues and downward gates that exchange it.</summary>
/// <param name="chozoStatue">The Chozo statues.</param>
/// <param name="downwardGate">The downward gates.</param>
internal sealed class EnemyObjectRequests(ChozoStatueBehavior chozoStatue, DownwardGateBehavior downwardGate) : IEnemyObjectRequests
{
    /// <inheritdoc/>
    public void ApplyPendingChozoStatueObjects(RoomLevelData level, ChozoStatueObjects chozoStatueObjects) =>
        chozoStatue.ApplyPendingChozoStatueObjects(level, chozoStatueObjects);

    /// <inheritdoc/>
    public void ActivateChozoStatueHandTrigger(RoomLevelData level, int collisionBlockIndex) =>
        chozoStatue.ActivateChozoStatueHandTrigger(level, collisionBlockIndex);

    /// <inheritdoc/>
    public void ApplyDownwardGateProjectileRequest(DownwardGateProjectileRequest request, int roomWidthInBlocks) =>
        downwardGate.ApplyDownwardGateProjectileRequest(request, roomWidthInBlocks);
}
