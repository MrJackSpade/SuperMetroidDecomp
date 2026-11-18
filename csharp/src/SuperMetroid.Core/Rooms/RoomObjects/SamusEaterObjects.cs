using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The Brinstar plants that catch Samus: their spawn, and what they do to the held Samus.</summary>
[RoomLifetime]
internal sealed class SamusEaterObjects
{
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess _samus;
    /// <summary>The room's places, where the plant's room object takes the first free one in update order.</summary>
    private readonly RoomObjectPlaceList _places;

    /// <summary>Binds the active room and its room-object places.</summary>
    internal SamusEaterObjects(IRoomSamusAccess samus, RoomObjectPlaceList places)
    {
        _samus = samus;
        _places = places;
    }
    /// <summary>Runs the feet/head alignment gate and allocates <paramref name="definition"/>'s plant room object.</summary>
    internal void TrySpawnSamusEater(RoomLevelData level,
        RoomCollisionBlock block, SamusEaterObjectDefinition definition, IRoomSamus samus)
    {
        Ensure.NotNull(definition);
        // Rejected setups free their just-taken place without changing
        // the trigger. Alignment is tested against Samus, not the sampled block row.
        int edge = definition.Ceiling ? samus.Position.Y.Whole - samus.Radius.Y
            : samus.Position.Y.Whole + samus.Radius.Y - 1;
        int fromAlignedFace = definition.Ceiling ? RoomGridConversions.OffsetWithinBlock(edge) : RoomGridConversions.OffsetFromBlockEnd(edge);
        if (fromAlignedFace != 0) return;
        // The spawn simply returns when every place is held.
        if (_places.Spawn() is not { } place)
            return;

        place.Block = block.At;
        place.Become(new SamusEaterObject(this, definition,
            samus.Position.Whole + new Offset16<RoomPixels>(0, definition.Ceiling ? 1 : -1)));
        level.SetTerrain(block.At, block.Terrain.Deactivated());
    }

    /// <summary>Holds Samus at <paramref name="held"/> after her normal movement and sustains her invincibility.</summary>
    internal void HoldSamus(Position16<RoomPixels> held)
    {
        var samus = _samus.Current;
        samus.Position = samus.Position.WithWhole(new(held.X, held.Y));
        // The hold sustains the invincibility timer without restarting it, not a clamp or an input lock. Samus's normal
        // movement runs before this placement, preserving interactions.
        samus.InvincibilityTimer = samus.InvincibilityTimer.Sustained(SamusEaterObjectDefinitions.HeldInvincibilityFrames);
    }

    /// <summary>One chew's periodic damage to the held Samus.</summary>
    internal void DamageSamus() => _samus.Current.TakePeriodicDamage(SamusEaterObjectDefinitions.ChewDamage);

    /// <summary>Gives the released Samus her release immunity.</summary>
    internal void ReleaseSamus() =>
        _samus.Current.InvincibilityTimer = WrappingCountdown.Start(SamusEaterObjectDefinitions.ReleaseInvincibilityFrames);
}
