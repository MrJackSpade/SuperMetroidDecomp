namespace SuperMetroid.Core.Rooms;

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
        for (int index = _slotTable._slots.Length - 1; index >= 0; index--)
        {
            PlmSlot slot = _slotTable._slots[index];
            if (slot.Active) continue;
            PlmPopulationLoader.ClearSlot(slot);
            slot.Active = true;
            slot.HeaderPointer = PlmHeaderId.EnemyBreakableBlock;
            slot.BlockIndex = blockIndex;
            slot.InstructionPointer = EnemyBreakableTerrainDefinitions.InstructionList;
            slot.InstructionTimer = NativeCountdown.Start(1);
            level.SetForegroundEntry(blockIndex, new RoomLevelWord(block.LevelWord)
                .WithCollisionType(RoomCollisionType.Air).Raw);
            return true;
        }
        // Native full-pool allocation skips setup. Its caller still returns carry clear.
        return false;
    }
}
