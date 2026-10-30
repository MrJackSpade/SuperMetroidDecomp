using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies;

/// <summary>Terrain PLMs that enemies break, spawned by enemy contact instead of Samus or projectiles.</summary>
internal sealed class EnemyBreakableTerrainPlms
{
    /// <summary>Finite native PLM slots searched in descending order for a free break effect.</summary>
    private readonly PlmSlotTable _slotTable;

    /// <summary>Uses the room's live PLM pool so enemy-triggered breaks share native allocation limits.</summary>
    internal EnemyBreakableTerrainPlms(PlmSlotTable slotTable)
    {
        _slotTable = slotTable;
    }
    /// <summary>
    /// Spawn_PLM and $84:B3D4: allocate a slot, clear collision immediately, and retain
    /// artwork until the PLM draws. A full pool skips setup and reports failure.
    /// </summary>
    public bool TrySpawnEnemyBreakableBlock(RoomLevelData level, int blockIndex)
    {
        Ensure.NotNull(level);
        RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
        // Native full-pool allocation skips setup. Its caller still returns carry clear.
        if (_slotTable.SpawnAtCurrentBlock() is not { } slot)
            return false;

        slot.Header = PlmHeaderId.EnemyBreakableBlock;
        slot.BlockIndex = blockIndex;
        slot.StartAnimation(new ScriptedRoomPlmAnimation(ScriptedRoomPlmProgram.EnemyBreakableTerrain));
        slot.InstructionTimer = WrappingCountdown.Start(1);
        level.SetTerrain(blockIndex, block.Terrain.WithCollision(RoomCollisionType.Air));
        return true;
    }
}
