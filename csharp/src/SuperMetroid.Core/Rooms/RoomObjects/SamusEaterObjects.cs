using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The Brinstar plants that catch Samus: their spawn, and what they do to the held Samus.</summary>
[RoomLifetime]
internal sealed class SamusEaterObjects
{
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess? _samus;
    /// <summary>Descending room-object slot allocation table.</summary>
    private readonly RoomObjectSlotTable _slotTable;

    /// <summary>Binds the active room and its room-object slot population.</summary>
    internal SamusEaterObjects(IRoomSamusAccess? samus, RoomObjectSlotTable slotTable)
    {
        _samus = samus;
        _slotTable = slotTable;
    }
    /// <summary>Runs the feet/head alignment gate and allocates <paramref name="definition"/>'s plant room object.</summary>
    internal void TrySpawnSamusEater(RoomLevelData level,
        RoomCollisionBlock block, SamusEaterObjectDefinition definition, IRoomSamus samus)
    {
        Ensure.NotNull(definition);
        // Rejected setups clear their just-allocated slot without changing
        // the trigger. Alignment is tested against Samus, not the sampled block row.
        int edge = definition.Ceiling ? samus.Position.Y.Whole - samus.Radius.Y
            : samus.Position.Y.Whole + samus.Radius.Y - 1;
        if ((edge & 15) != (definition.Ceiling ? 0 : 15)) return;
        // Descending allocation simply returns when all slots are occupied.
        if (_slotTable.Spawn() is not { } slot)
            return;

        slot.Header = definition.Identity;
        slot.Block = level.BlockAt(block.Index);
        slot.Become(new SamusEaterObject(this, definition,
            new(samus.Position.X.Whole, CoordinateAxis.Wrap(samus.Position.Y.Whole + (definition.Ceiling ? 1 : -1)))));
        level.SetTerrain(block.Index, block.Terrain.Deactivated());
    }

    /// <summary>Requires the live Samus instance associated with the plant's room.</summary>
    internal IRoomSamus RequirePlantSamus() => _samus?.Current
        ?? throw new InvalidOperationException("Samus Eater room object requires the live Samus owner.");

    /// <summary>Holds Samus at <paramref name="held"/> after her normal movement and sustains her invincibility.</summary>
    internal void HoldSamus(Position16<RoomPixels> held)
    {
        var samus = RequirePlantSamus();
        samus.Position = samus.Position.WithWhole(new(held.X, held.Y));
        // The hold sustains the invincibility timer without restarting it, not a clamp or an input lock. Samus's normal
        // movement runs before this placement, preserving interactions.
        samus.InvincibilityTimer = samus.InvincibilityTimer.Sustained(SamusEaterObjectDefinitions.HeldInvincibilityFrames);
    }

    /// <summary>One chew's periodic damage to the held Samus.</summary>
    internal void DamageSamus() => RequirePlantSamus().TakePeriodicDamage(0, SamusEaterObjectDefinitions.ChewDamage);

    /// <summary>Gives the released Samus her release immunity.</summary>
    internal void ReleaseSamus() =>
        RequirePlantSamus().InvincibilityTimer = WrappingCountdown.Start(SamusEaterObjectDefinitions.ReleaseInvincibilityFrames);
}
