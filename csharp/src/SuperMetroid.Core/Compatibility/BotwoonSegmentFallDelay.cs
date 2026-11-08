namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// When each of Botwoon's body segments falls after he dies (#1542): a segment's count toward falling starts by the
/// enemy-projectile place it was spawned into, not by its place in his body, so segments in later projectile places
/// start higher and fall sooner. The last segment to land ends his death, so the order the projectile places were
/// handed out sets when his death finishes.
/// </summary>
/// <remarks>
/// Consulted once per segment, on the update its death starts, before its count first advances. Evidence: the
/// segment's death setup loads its count from its projectile index, 96 plus eight for each place; the body spawns hand
/// the segments the free projectile places in the allocator's own order.
/// </remarks>
[CompatibilityService("Botwoon's dying body segments fall in the order of the enemy-projectile places they were spawned into, later places sooner")]
public static class BotwoonSegmentFallDelay
{
    /// <summary>The count a dying segment in the first projectile place starts at.</summary>
    private const int FirstPlaceStart = 96;

    /// <summary>How much higher each later projectile place starts its dying segment's count.</summary>
    private const int PerPlace = 8;

    /// <summary>The count toward falling a segment in projectile place <paramref name="projectilePlace"/> starts at.</summary>
    /// <param name="projectilePlace">The segment's zero-based enemy-projectile place.</param>
    /// <exception cref="ArgumentOutOfRangeException">The place is negative.</exception>
    public static int Start(int projectilePlace) => FirstPlaceStart + PerPlace * Ensure.AtLeastZero(projectilePlace);
}
