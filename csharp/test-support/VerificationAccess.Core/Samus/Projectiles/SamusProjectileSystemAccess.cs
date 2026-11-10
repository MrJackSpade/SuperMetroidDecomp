using System.Reflection;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Verification access to <see cref="SamusProjectileSystem"/> members production does not use.</summary>
internal static class SamusProjectileSystemAccess
{
    /// <summary>Reads the trail-slot pool in native byte-index order and counts active left streams.</summary>
    extension(SamusProjectileSystem self)
    {
        /// <summary>
        /// The independent trail pool in native low-to-high byte-index order. Allocation scans
        /// this collection backward, matching <c>$90:B679-$B683</c> rather than using a queue.
        /// </summary>
        internal IReadOnlyList<SamusProjectileTrailSlot> TrailSlots => TrailSlotsOf(self.SlotTable);

        /// <summary>Debugger-friendly count of slots whose left stream still owns the slot.</summary>
        internal int ActiveTrailCount => TrailSlotsOf(self.SlotTable).Count(slot => slot.IsActive);
    }

    /// <summary>The table's trail slots in ordinal order, which only checks read whole (#1590).</summary>
    private static SamusProjectileTrailSlot[] TrailSlotsOf(ProjectileSlotTable table) =>
        typeof(ProjectileSlotTable).GetField("_trailSlots", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(table) as SamusProjectileTrailSlot[]
            ?? throw new InvalidOperationException("ProjectileSlotTable no longer keeps its trail slots in _trailSlots.");
}
