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
        internal IReadOnlyList<SamusProjectileTrailSlot> TrailSlots => self.SlotTable._trailSlots;

        /// <summary>Debugger-friendly count of slots whose left stream still owns the slot.</summary>
        internal int ActiveTrailCount => self.SlotTable._trailSlots.Count(slot => slot.IsActive);
    }
}
