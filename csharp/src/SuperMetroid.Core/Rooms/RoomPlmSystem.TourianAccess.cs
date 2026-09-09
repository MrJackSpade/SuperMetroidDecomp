using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

public sealed partial class RoomPlmSystem
{
    /// <summary>$88:DB8A/DC69 spawn the native clear/crumble lists at block (6,12).</summary>
    public bool TrySpawnTourianAccess(ISnesAddressSpace bus, RoomLevelData level, bool clear)
    {
        ushort header = clear ? TourianStatueRomData.ClearAccess : TourianStatueRomData.CrumbleAccess;
        for (int index = _slots.Length - 1; index >= 0; index--)
        {
            var slot = _slots[index];
            if (slot.Active) continue;
            ClearSlot(slot);
            slot.Active = true;
            slot.HeaderPointer = header;
            slot.BlockIndex = level.GetBlockIndex(6, 12);
            slot.InstructionPointer = (ushort)(bus.ReadByte(TourianStatueRomData.AccessPlmBank | (header + 2)) |
                bus.ReadByte(TourianStatueRomData.AccessPlmBank | (header + 3)) << 8);
            slot.InstructionTimer = 1;
            return true;
        }
        return false;
    }
}
