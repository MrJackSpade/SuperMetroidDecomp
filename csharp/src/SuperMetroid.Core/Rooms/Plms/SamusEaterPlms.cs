using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Samus-eater plant PLMs: their spawn, the held Samus position and the pre-instruction that runs the bite.</summary>
[RoomLifetime]
internal sealed class SamusEaterPlms
{
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess? _samus;
    /// <summary>Descending native PLM slot allocation table.</summary>
    private readonly PlmSlotTable _slotTable;

    /// <summary>Binds the active room and its PLM slot population.</summary>
    internal SamusEaterPlms(IRoomSamusAccess? samus, PlmSlotTable slotTable)
    {
        _samus = samus;
        _slotTable = slotTable;
    }
    /// <summary>Runs the native feet/head alignment gate and allocates the ordinary ROM coroutine.</summary>
    internal void TrySpawnSamusEater(RoomLevelData level,
        RoomCollisionBlock block, PlmHeaderId header, IRoomSamus samus)
    {
        SamusEaterPlmDefinition definition = SamusEaterPlmDefinitions.Resolve(header);
        // Rejected native setups clear their just-allocated slot without changing
        // the trigger. Alignment is tested against Samus, not the sampled block row.
        int edge = definition.Ceiling ? samus.Position.Y.Whole - samus.Radius.Y
            : samus.Position.Y.Whole + samus.Radius.Y - 1;
        if ((edge & 15) != (definition.Ceiling ? 0 : 15)) return;
        // Native descending allocation simply returns when all slots are occupied.
        if (_slotTable.SpawnAtCurrentBlock() is not { } slot)
            return;

        slot.Header = header;
        slot.BlockIndex = block.Index;
        slot.StartAnimation(new SamusEaterPlmAnimation(definition.Program));
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.PlantHeldPosition = new(samus.Position.X.Whole, unchecked((ushort)(samus.Position.Y.Whole + (definition.Ceiling ? 1 : -1))));
        level.SetForegroundEntry(block.Index, block.PackedWord.WithPlmDeactivation().Raw);
        return;
    }

    /// <summary>Requires the live Samus instance associated with the plant's room.</summary>
    internal IRoomSamus RequirePlantSamus() => _samus?.Current
        ?? throw new InvalidOperationException("Samus Eater PLM requires the live Samus owner.");

    /// <summary>Repositions held Samus after normal movement and ORs in the native invincibility bit.</summary>
    internal void RunSamusEaterPreInstruction(PlmSlot slot)
    {
        var samus = RequirePlantSamus();
        samus.Position = samus.Position.WithWhole(new(slot.PlantHeldPosition.X, slot.PlantHeldPosition.Y));
        // Native TSB is bitwise OR, not a clamp or an input lock. Samus's normal
        // movement runs before this placement, preserving cartridge interactions.
        samus.InvincibilityTimer = WrappingCountdown.FromRaw(unchecked((ushort)(samus.InvincibilityTimer.RawValue | SamusEaterPlmDefinitions.HeldInvincibilityBit)));
    }

    /// <summary><c>$84:AC9D</c>: one chew's periodic damage to the held Samus.</summary>
    internal void DamageSamus() => RequirePlantSamus().TakePeriodicDamage(0, SamusEaterPlmDefinitions.ChewDamage);

    /// <summary><c>$84:ACB1</c>: gives the released Samus her release immunity.</summary>
    internal void ReleaseSamus() =>
        RequirePlantSamus().InvincibilityTimer = WrappingCountdown.Start(SamusEaterPlmDefinitions.ReleaseInvincibilityFrames);
}
