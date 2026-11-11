using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies;

/// <summary>Terrain room objects that enemies break, spawned by enemy contact instead of Samus or projectiles.</summary>
internal sealed class EnemyBreakableTerrainObjects
{
    /// <summary>Finite room-object slots searched in descending order for a free break effect.</summary>
    private readonly RoomObjectSlotTable _slotTable;

    /// <summary>Uses the room's live room-object pool so enemy-triggered breaks share allocation limits.</summary>
    internal EnemyBreakableTerrainObjects(RoomObjectSlotTable slotTable)
    {
        _slotTable = slotTable;
    }
    /// <summary>
    /// Spawns the breakable block's break: take a place, clear the block's collision at once and keep its artwork until
    /// the break draws. A full room skips the setup and reports failure.
    /// </summary>
    public bool TrySpawnEnemyBreakableBlock(RoomLevelData level, Position16<RoomBlocks> position)
    {
        Ensure.NotNull(level);
        RoomCollisionBlock block = level.GetCollisionBlock(position);
        // Full-pool allocation skips setup. Its caller still returns carry clear.
        if (_slotTable.SpawnAtCurrentBlock() is not { } slot)
            return false;

        slot.Header = RoomObjectHeaderId.EnemyBreakableBlock;
        slot.Block = position;
        slot.Become(new TerrainChangeObject(ScriptedTerrainChangeDefinitions.EnemyBreakableBlockBreak));
        level.SetTerrain(position, block.Terrain.WithCollision(RoomCollisionType.Air));
        return true;
    }
}
