namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// When each of Botwoon's body segments falls after he dies (#1542): a segment's wait before it falls follows the
/// enemy-projectile place it was spawned into, not its place in his body, so segments in later projectile places wait
/// less and fall sooner. The last segment to land ends his death, so the order the projectile places were handed out
/// sets when his death finishes.
/// </summary>
/// <remarks>
/// Consulted once per segment, on the update its death starts, which is the wait's first update. Evidence: the segment's
/// death setup loads a count from its projectile index, 96 plus eight for each place, and the segment falls on the update
/// its count reaches 256, the setup's update counting; the body spawns hand the segments the free projectile places in
/// the allocator's own order.
/// </remarks>
[CompatibilityService("Botwoon's dying body segments fall in the order of the enemy-projectile places they were spawned into, later places sooner")]
public static class BotwoonSegmentFallDelay
{
    /// <summary>The updates a dying segment in the first projectile place waits before it falls.</summary>
    private const int FirstPlaceWaitUpdates = 160;

    /// <summary>How many updates less each later projectile place waits.</summary>
    private const int PerPlace = 8;

    /// <summary>
    /// The updates a dying segment in projectile place <paramref name="projectilePlace"/> waits before it falls, its death's
    /// first update counting: it falls from the update after the last.
    /// </summary>
    /// <param name="projectilePlace">The segment's zero-based enemy-projectile place.</param>
    /// <exception cref="ArgumentOutOfRangeException">The place is negative.</exception>
    public static int WaitUpdates(int projectilePlace) => FirstPlaceWaitUpdates - PerPlace * Ensure.AtLeastZero(projectilePlace);
}
