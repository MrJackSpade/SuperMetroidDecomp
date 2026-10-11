using SuperMetroid.Core.Game;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

public sealed partial class RoomPlmSystem
{
    /// <summary>Runs the native feet/head alignment gate and allocates the ordinary ROM coroutine.</summary>
    internal void TrySpawnSamusEater(RoomLevelData level,
        RoomCollisionBlock block, PlmHeaderId header, SamusState samus)
    {
        SamusEaterPlmDefinition definition = SamusEaterPlmDefinitions.Resolve(header);
        // Rejected native setups clear their just-allocated slot without changing
        // the trigger. Alignment is tested against Samus, not the sampled block row.
        int edge = definition.Ceiling ? samus.Position.Y.Whole - samus.Kinematics.Radius.Y
            : samus.Position.Y.Whole + samus.Kinematics.Radius.Y - 1;
        if ((edge & 15) != (definition.Ceiling ? 0 : 15)) return;
        for (int index = _slots.Length - 1; index >= 0; index--)
        {
            var slot = _slots[index];
            if (slot.Active) continue;
            ClearSlot(slot);
            slot.Active = true;
            slot.HeaderPointer = header;
            slot.BlockIndex = block.Index;
            slot.InstructionPointer = definition.InstructionListPointer;
            slot.InstructionTimer = 1;
            slot.PlantHeldPosition = new(samus.Position.X.Whole, unchecked((ushort)(samus.Position.Y.Whole + (definition.Ceiling ? 1 : -1))));
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
        samus.Position = samus.Position.WithWhole(new(slot.PlantHeldPosition.X, slot.PlantHeldPosition.Y));
        // Native TSB is bitwise OR, not a clamp or an input lock. Samus's normal
        // movement runs before this placement, preserving cartridge interactions.
        samus.InvincibilityTimer |= 0x10;
    }
}
