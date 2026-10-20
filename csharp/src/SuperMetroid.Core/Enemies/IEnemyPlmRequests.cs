using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies;

/// <summary>The PLM work enemies and their PLMs hand each other each frame.</summary>
internal interface IEnemyPlmRequests
{
    /// <summary>Applies the Chozo statue PLM work the statues requested this frame.</summary>
    void ApplyPendingChozoStatuePlms(RoomLevelData level, ChozoStatuePlms chozoStatuePlms);

    /// <summary>Spawns or wakes the downward gate's projectile a gate PLM requested.</summary>
    void ApplyDownwardGateProjectileRequest(DownwardGateProjectileRequest request, int roomWidthInBlocks);
}
