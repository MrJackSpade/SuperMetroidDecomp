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

    /// <summary>
    /// Runs room setup $8F:9194's Spawn_Hardcoded_PLM of $84:BB30 at block ($3D,$0B). Setup
    /// $84:BB09 clears the PLM ID unless event $0F (critters escaped) is set, so the passage
    /// stays shut when the animals were left behind.
    /// </summary>
    /// <returns>False when no PLM remains: the event is unset or every slot is occupied.</returns>
    public bool TrySpawnCrateriaMainstreetEscapePassage(RoomLevelData level)
    {
        ArgumentNullException.ThrowIfNull(level);
        Func<EventNumber, bool> hasEvent = _hasEvent ?? throw new InvalidOperationException(
            "Crateria mainstreet escape passage requires the room event owner.");
        if (_slots.All(slot => slot.Active))
            return false;
        if (!hasEvent(EventNumber.CrittersEscaped))
            return false;
        for (int index = _slots.Length - 1; index >= 0; index--)
        {
            PlmSlot slot = _slots[index];
            if (slot.Active)
                continue;
            ClearSlot(slot);
            slot.Active = true;
            slot.BlockIndex = level.GetBlockIndex(
                CrateriaMainstreetEscapePassagePlmDefinitions.BlockX,
                CrateriaMainstreetEscapePassagePlmDefinitions.BlockY);
            slot.RestoreLevelWord = 0;
            slot.HeaderPointer = PlmHeaderId.CrateriaMainstreetEscapePassage;
            slot.LoopTimer = 0;
            slot.InstructionPointer = CrateriaMainstreetEscapePassagePlmDefinitions.InstructionList;
            slot.InstructionTimer = 1;
            return true;
        }
        return false;
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
            slot.RestoreLevelWord = (ushort)((block.LevelWord & EscapeAnimalPlmRomData.ReactionCollisionMask) | EscapeAnimalPlmRomData.ReactionVisualBlock);
            level.SetForegroundEntry(blockIndex, (ushort)(slot.RestoreLevelWord & EscapeAnimalPlmRomData.ReactionTemporaryCollisionMask));
            return true;
        }
        return false;
    }
}
