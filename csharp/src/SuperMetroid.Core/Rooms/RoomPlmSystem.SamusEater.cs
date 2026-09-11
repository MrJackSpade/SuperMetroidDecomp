using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

public sealed partial class RoomPlmSystem
{
    /// <summary>Runs the native feet/head alignment gate and allocates the ordinary ROM coroutine.</summary>
    internal void TrySpawnSamusEater(ISnesAddressSpace bus, RoomLevelData level,
        RoomCollisionBlock block, ushort header, bool ceiling, SamusState samus)
    {
        // Rejected native setups clear their just-allocated slot without changing
        // the trigger. Alignment is tested against Samus, not the sampled block row.
        int edge = ceiling ? samus.YPosition - samus.Kinematics.YRadius
            : samus.YPosition + samus.Kinematics.YRadius - 1;
        if ((edge & 15) != (ceiling ? 0 : 15)) return;
        for (int index = _slots.Length - 1; index >= 0; index--)
        {
            var slot = _slots[index];
            if (slot.Active) continue;
            ClearSlot(slot);
            slot.Active = true;
            slot.HeaderPointer = header;
            slot.BlockIndex = block.Index;
            slot.InstructionPointer = ReadBank84Word(bus, unchecked((ushort)(header + 2)));
            slot.InstructionTimer = 1;
            slot.PlantHeldX = samus.XPosition;
            slot.PlantHeldY = unchecked((ushort)(samus.YPosition + (ceiling ? 1 : -1)));
            level.SetForegroundEntry(block.Index, (ushort)(block.LevelWord & SamusEaterPlmRomData.DeactivatedTriggerMask));
            return;
        }
        // Native descending allocation simply returns when all slots are occupied.
    }

    private SamusState RequirePlantSamus() => _collectibleSamus?.Invoke()
        ?? throw new InvalidOperationException("Samus Eater PLM requires the live Samus owner.");

    private void RunSamusEaterPreInstruction(PlmSlot slot)
    {
        if (slot.PreInstruction != SamusEaterPlmRomData.HoldPreInstruction) return;
        var samus = RequirePlantSamus();
        samus.XPosition = slot.PlantHeldX;
        samus.YPosition = slot.PlantHeldY;
        // Native TSB is bitwise OR, not a clamp or an input lock. Samus's normal
        // movement runs before this placement, preserving cartridge interactions.
        samus.InvincibilityTimer |= 0x10;
    }
}
