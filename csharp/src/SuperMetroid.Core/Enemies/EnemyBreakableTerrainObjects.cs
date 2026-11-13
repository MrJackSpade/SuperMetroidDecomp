using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies;

/// <summary>Terrain room objects that enemies break, spawned by enemy contact instead of Samus or projectiles.</summary>
internal sealed class EnemyBreakableTerrainObjects
{
    /// <summary>The room's room objects, which an enemy's break joins.</summary>
    private readonly RoomObjectSlotTable _slotTable;

    /// <summary>Uses the room's room objects, so an enemy's break takes a place like every other room object.</summary>
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
        // A full room skips the setup, and the block is reported as not broken.
        if (_slotTable.Spawn() is not { } slot)
            return false;

        slot.Block = position;
        slot.Become(new TerrainChangeObject(ScriptedTerrainChangeDefinitions.EnemyBreakableBlockBreak));
        level.SetTerrain(position, block.Terrain.WithCollision(RoomCollisionType.Air));
        return true;
    }
}
