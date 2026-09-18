namespace SuperMetroid.Core.Rooms;

public sealed partial class RoomPlmSystem
{
    /// <summary>$88:DB8A/DC69 spawn the native clear/crumble lists at block (6,12).</summary>
    public bool TrySpawnTourianAccess(RoomLevelData level, bool clear)
    {
        TourianAccessPlmDefinition definition = TourianAccessPlmDefinitions.ForState(clear);
        for (int index = _slots.Length - 1; index >= 0; index--)
        {
            var slot = _slots[index];
            if (slot.Active) continue;
            ClearSlot(slot);
            slot.Active = true;
            slot.HeaderPointer = definition.HeaderPointer;
            slot.BlockIndex = level.GetBlockIndex(6, 12);
            slot.InstructionPointer = definition.InstructionListPointer;
            slot.InstructionTimer = 1;
            return true;
        }
        return false;
    }
}
