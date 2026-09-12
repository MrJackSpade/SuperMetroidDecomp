namespace SuperMetroid.Core.Rooms;

public sealed partial class RoomPlmSystem
{
    /// <summary>Spawn_PLM and $84:B3D4: clear collision immediately, retain artwork until the PLM draws.</summary>
    public bool TrySpawnEnemyBreakableBlock(RoomLevelData level, int blockIndex)
    {
        ArgumentNullException.ThrowIfNull(level);
        RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
        for (int index = _slots.Length - 1; index >= 0; index--)
        {
            PlmSlot slot = _slots[index];
            if (slot.Active) continue;
            ClearSlot(slot);
            slot.Active = true;
            slot.HeaderPointer = EnemyBreakableTerrainDefinitions.Header;
            slot.BlockIndex = blockIndex;
            slot.InstructionPointer = EnemyBreakableTerrainDefinitions.InstructionList;
            slot.InstructionTimer = 1;
            level.SetForegroundEntry(blockIndex, new RoomLevelWord(block.LevelWord)
                .WithCollisionType(RoomCollisionType.Air).Raw);
            return true;
        }
        // Native full-pool allocation skips setup. Its caller still returns carry clear.
        return false;
    }
}
