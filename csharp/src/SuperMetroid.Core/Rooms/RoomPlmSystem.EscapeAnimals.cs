using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

public sealed partial class RoomPlmSystem
{
    /// <summary>Runs $84:B9C5: make the three-block rescue wall shootable, preserving artwork.</summary>
    public void SetupCrittersEscapeBlock(RoomLevelData level, int blockIndex)
    {
        // Hardcoded spawn cannot execute setup when every native PLM slot is occupied.
        if (_slots.All(slot => slot.Active))
            throw new InvalidOperationException("Animal rescue wall setup has no free PLM slot.");
        WritePlmCollisionTypeAndBts(level, blockIndex, EscapeAnimalPlmRomData.OriginCollision);
        for (int row = 1; row <= 2; row++)
            WritePlmCollisionTypeAndBts(level, blockIndex + row * level.WidthInBlocks,
                EscapeAnimalPlmRomData.ExtensionCollision);
    }

    /// <summary>Runs $84:B978 and schedules its ROM animation/event list in native slot order.</summary>
    private bool TrySpawnCrittersEscapeReaction(RoomLevelData level, int blockIndex,
        SamusProjectileTypeWord projectileType)
    {
        // The native branch tests the complete projectile word, not its family. A zero
        // grapple word is rejected; ordinary nonzero beams and bombs are accepted.
        if (projectileType == (SamusProjectileTypeWord)0)
            return false;
        for (int index = _slots.Length - 1; index >= 0; index--)
        {
            PlmSlot slot = _slots[index];
            if (slot.Active) continue;
            ClearSlot(slot);
            slot.Active = true;
            slot.BlockIndex = blockIndex;
            slot.InstructionPointer = EscapeAnimalPlmRomData.ReactionList;
            slot.InstructionTimer = 1;
            RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
            slot.RestoreLevelWord = (ushort)((block.LevelWord & 0xf000) | EscapeAnimalPlmRomData.ReactionVisualBlock);
            level.SetForegroundEntry(blockIndex, (ushort)(slot.RestoreLevelWord & 0x8fff));
            return true;
        }
        return false;
    }
}
