using SuperMetroid.Core.Game;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>Samus-eater plant PLMs: their spawn, the held Samus position and the pre-instruction that runs the bite.</summary>
internal sealed class SamusEaterPlms
{
    /// <summary>Stores bindings.</summary>
    private readonly PlmRoomBindings _bindings;
    /// <summary>Stores slot table.</summary>
    private readonly PlmSlotTable _slotTable;

    /// <summary>Initializes a new samus eater plms instance.</summary>
    internal SamusEaterPlms(PlmRoomBindings bindings, PlmSlotTable slotTable)
    {
        _bindings = bindings;
        _slotTable = slotTable;
    }
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
        for (int index = _slotTable._slots.Length - 1; index >= 0; index--)
        {
            var slot = _slotTable._slots[index];
            if (slot.Active) continue;
            PlmPopulationLoader.ClearSlot(slot);
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

    /// <summary>require plant samus.</summary>
    internal SamusState RequirePlantSamus() => _bindings.Samus?.Current
        ?? throw new InvalidOperationException("Samus Eater PLM requires the live Samus owner.");

    /// <summary>run samus eater pre instruction.</summary>
    internal void RunSamusEaterPreInstruction(PlmSlot slot)
    {
        if (slot.PreInstruction != SamusEaterPlmRomData.HoldPreInstruction) return;
        var samus = RequirePlantSamus();
        samus.Position = samus.Position.WithWhole(new(slot.PlantHeldPosition.X, slot.PlantHeldPosition.Y));
        // Native TSB is bitwise OR, not a clamp or an input lock. Samus's normal
        // movement runs before this placement, preserving cartridge interactions.
        samus.InvincibilityTimer |= 0x10;
    }
}
