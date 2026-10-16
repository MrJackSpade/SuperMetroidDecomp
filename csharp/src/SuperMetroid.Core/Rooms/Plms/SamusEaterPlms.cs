using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Samus-eater plant PLMs: their spawn, the held Samus position and the pre-instruction that runs the bite.</summary>
internal sealed class SamusEaterPlms
{
    /// <summary>Live room owners used to find held Samus and allocate PLM slots.</summary>
    private readonly PlmRoomBindings _bindings;
    /// <summary>Descending native PLM slot allocation table.</summary>
    private readonly PlmSlotTable _slotTable;

    /// <summary>Binds the active room and its PLM slot population.</summary>
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
            slot.Clear();
            slot.Active = true;
            slot.HeaderPointer = header;
            slot.BlockIndex = block.Index;
            slot.StartAnimation(new SamusEaterPlmAnimation(definition.Program));
            slot.InstructionTimer = NativeCountdown.Start(1);
            slot.PlantHeldPosition = new(samus.Position.X.Whole, unchecked((ushort)(samus.Position.Y.Whole + (definition.Ceiling ? 1 : -1))));
            level.SetForegroundEntry(block.Index, (ushort)(block.LevelWord & SamusEaterPlmRomData.DeactivatedTriggerMask));
            return;
        }
        // Native descending allocation simply returns when all slots are occupied.
    }

    /// <summary>Requires the live Samus instance associated with the plant's room.</summary>
    internal SamusState RequirePlantSamus() => _bindings.Samus?.Current
        ?? throw new InvalidOperationException("Samus Eater PLM requires the live Samus owner.");

    /// <summary>Repositions held Samus after normal movement and ORs in the native invincibility bit.</summary>
    internal void RunSamusEaterPreInstruction(PlmSlot slot)
    {
        var samus = RequirePlantSamus();
        samus.Position = samus.Position.WithWhole(new(slot.PlantHeldPosition.X, slot.PlantHeldPosition.Y));
        // Native TSB is bitwise OR, not a clamp or an input lock. Samus's normal
        // movement runs before this placement, preserving cartridge interactions.
        samus.InvincibilityTimer = NativeCountdown.FromRaw(unchecked((ushort)(samus.InvincibilityTimer.RawValue | SamusEaterPlmRomData.HeldInvincibilityBit)));
    }

    /// <summary><c>$84:AC9D</c>: one chew's periodic damage to the held Samus.</summary>
    internal void DamageSamus() => RequirePlantSamus().LiquidPhysics.AccumulatePeriodicDamage(0, SamusEaterPlmRomData.ChewDamage);

    /// <summary><c>$84:ACB1</c>: gives the released Samus her release immunity.</summary>
    internal void ReleaseSamus() =>
        RequirePlantSamus().InvincibilityTimer = NativeCountdown.Start(SamusEaterPlmRomData.ReleaseInvincibilityFrames);
}
