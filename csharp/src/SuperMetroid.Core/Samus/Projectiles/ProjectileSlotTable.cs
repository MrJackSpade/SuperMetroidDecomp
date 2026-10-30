namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Samus's ordinary projectile slots, the trail slots and the projectile counter.</summary>
/// <remarks>
/// Slot order: Samus has <see cref="SamusProjectileSystem.SlotCount"/> shot slots and
/// <see cref="SamusProjectileSystem.TrailSlotCount"/> trail slots, each with a fixed ordinal. A shot takes the lowest
/// free ordinal; the projectile update, collision and drawing run in update order, from the highest ordinal down. A trail
/// takes the first free trail slot in update order. Some projectile behaviour names slots by ordinal (the Spazer
/// combination's paired particles, an unlinked Super Missile's slot zero), so the ordinal is part of a shot's identity.
/// </remarks>
internal sealed class ProjectileSlotTable
{
    /// <summary>The shot slots by ordinal; their occupancy does not replace the projectile counter.</summary>
    private readonly SamusProjectileSlot[] _slots =
        Enumerable.Range(0, SamusProjectileSystem.SlotCount).Select(index => new SamusProjectileSlot(index)).ToArray();
    /// <summary>The trail slots by ordinal, independent of the shot slots.</summary>
    private readonly SamusProjectileTrailSlot[] _trailSlots =
        Enumerable.Range(0, SamusProjectileSystem.TrailSlotCount).Select(index => new SamusProjectileTrailSlot(index)).ToArray();

    /// <summary>The shot slots by ordinal.</summary>
    internal IReadOnlyList<SamusProjectileSlot> Slots => _slots;
    /// <summary>The lowest shot slot with no damage, which the firing routines take as free, or -1 when all are live.</summary>
    internal int FindFreeSlotIndex() => Array.FindIndex(_slots, slot => slot.Damage == 0);

    /// <summary>The shot slots in update order, highest ordinal first.</summary>
    internal IEnumerable<SamusProjectileSlot> InUpdateOrder
    {
        get
        {
            for (int ordinal = _slots.Length - 1; ordinal >= 0; ordinal--)
                yield return _slots[ordinal];
        }
    }

    /// <summary>The trail slots by ordinal.</summary>
    internal IReadOnlyList<SamusProjectileTrailSlot> TrailSlots => _trailSlots;

    /// <summary>The trail slots in update order, highest ordinal first; a new trail takes the first free one.</summary>
    internal IEnumerable<SamusProjectileTrailSlot> TrailsInUpdateOrder
    {
        get
        {
            for (int ordinal = _trailSlots.Length - 1; ordinal >= 0; ordinal--)
                yield return _trailSlots[ordinal];
        }
    }

    /// <summary>How many shots Samus has out by the firing routines' count; kept separately from the free-slot search.</summary>
    public int ProjectileCounter
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }
}
