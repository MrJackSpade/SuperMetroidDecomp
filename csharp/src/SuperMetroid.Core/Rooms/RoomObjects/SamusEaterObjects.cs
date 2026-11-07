using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Samus-eater plant PLMs: their spawn, the held Samus position and the pre-instruction that runs the bite.</summary>
[RoomLifetime]
internal sealed class SamusEaterObjects
{
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess? _samus;
    /// <summary>Descending PLM slot allocation table.</summary>
    private readonly RoomObjectSlotTable _slotTable;

    /// <summary>Binds the active room and its PLM slot population.</summary>
    internal SamusEaterObjects(IRoomSamusAccess? samus, RoomObjectSlotTable slotTable)
    {
        _samus = samus;
        _slotTable = slotTable;
    }
    /// <summary>Runs the feet/head alignment gate and allocates the ordinary plant PLM.</summary>
    internal void TrySpawnSamusEater(RoomLevelData level,
        RoomCollisionBlock block, RoomObjectHeaderId header, IRoomSamus samus)
    {
        SamusEaterObjectDefinition definition = Plant(header);
        // Rejected setups clear their just-allocated slot without changing
        // the trigger. Alignment is tested against Samus, not the sampled block row.
        int edge = definition.Ceiling ? samus.Position.Y.Whole - samus.Radius.Y
            : samus.Position.Y.Whole + samus.Radius.Y - 1;
        if ((edge & 15) != (definition.Ceiling ? 0 : 15)) return;
        // Descending allocation simply returns when all slots are occupied.
        if (_slotTable.SpawnAtCurrentBlock() is not { } slot)
            return;

        slot.Header = header;
        slot.BlockIndex = block.Index;
        slot.StartAnimation(new SamusEaterObjectAnimation(definition.Program));
        slot.FrameHold = 1;
        slot.PlantHeldPosition = new(samus.Position.X.Whole, CoordinateAxis.Wrap(samus.Position.Y.Whole + (definition.Ceiling ? 1 : -1)));
        level.SetTerrain(block.Index, block.Terrain.Deactivated());
        return;
    }

    /// <summary>The plant one supported header places, without interpreting adjacent data.</summary>
    internal static SamusEaterObjectDefinition Plant(RoomObjectHeaderId header) =>
        SamusEaterObjectDefinitions.ByHeader.TryGetValue(header, out SamusEaterObjectDefinition plant)
            ? plant
            : throw new InvalidDataException($"Unsupported Samus Eater PLM {header}.");

    /// <summary>Requires the live Samus instance associated with the plant's room.</summary>
    internal IRoomSamus RequirePlantSamus() => _samus?.Current
        ?? throw new InvalidOperationException("Samus Eater PLM requires the live Samus owner.");

    /// <summary>Repositions held Samus after normal movement and ORs in the invincibility bit.</summary>
    internal void RunSamusEaterFrameBehavior(RoomObjectSlot slot)
    {
        var samus = RequirePlantSamus();
        samus.Position = samus.Position.WithWhole(new(slot.PlantHeldPosition.X, slot.PlantHeldPosition.Y));
        // TSB is bitwise OR, not a clamp or an input lock. Samus's normal
        // movement runs before this placement, preserving interactions.
        samus.InvincibilityTimer = samus.InvincibilityTimer.MergedWith(SamusEaterObjectDefinitions.HeldInvincibilityBit);
    }

    /// <summary>One chew's periodic damage to the held Samus.</summary>
    internal void DamageSamus() => RequirePlantSamus().TakePeriodicDamage(0, SamusEaterObjectDefinitions.ChewDamage);

    /// <summary>Gives the released Samus her release immunity.</summary>
    internal void ReleaseSamus() =>
        RequirePlantSamus().InvincibilityTimer = WrappingCountdown.Start(SamusEaterObjectDefinitions.ReleaseInvincibilityFrames);
}
