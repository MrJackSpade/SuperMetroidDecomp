using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.World;

/// <summary>The PLM work over the Chozo statues and downward gates that exchange it.</summary>
/// <param name="chozoStatue">The Chozo statues.</param>
/// <param name="downwardGate">The downward gates.</param>
internal sealed class EnemyPlmRequests(ChozoStatueBehavior chozoStatue, DownwardGateBehavior downwardGate) : IEnemyPlmRequests
{
    /// <inheritdoc/>
    public void ApplyPendingChozoStatuePlms(RoomLevelData level, ChozoStatuePlms chozoStatuePlms) =>
        chozoStatue.ApplyPendingChozoStatuePlms(level, chozoStatuePlms);

    /// <inheritdoc/>
    public void ApplyDownwardGateProjectileRequest(DownwardGateProjectileRequest request, int roomWidthInBlocks) =>
        downwardGate.ApplyDownwardGateProjectileRequest(request, roomWidthInBlocks);
}
