namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Whether an enemy's finished death explosion turns into its drop (#1542): never in the first enemy-projectile place.
/// A death explosion that finishes in any other place becomes the pickup its drop roll chose; one in the first place
/// stays a dormant pickup whatever the roll, so an enemy whose explosion takes that place drops nothing. A drop spawned
/// directly, not from an explosion, becomes a pickup in any place.
/// </summary>
/// <remarks>
/// Consulted once per explosion, by its become-pickup cue after the drop roll. Evidence: the become-pickup cue branches
/// on the restored enemy-projectile index, and index zero takes the no-drop path.
/// </remarks>
[CompatibilityService("an enemy's death explosion in the first enemy-projectile place never becomes a pickup")]
public static class FirstPlaceExplosionLeavesNoPickup
{
    /// <summary>Whether a finished death explosion in the enemy-projectile place <paramref name="projectilePlace"/> becomes a pickup.</summary>
    /// <param name="projectilePlace">The zero-based enemy-projectile place the explosion occupies.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="projectilePlace"/> is negative.</exception>
    public static bool BecomesPickup(int projectilePlace) => Ensure.BetweenInclusive(0, int.MaxValue, projectilePlace) != 0;
}
