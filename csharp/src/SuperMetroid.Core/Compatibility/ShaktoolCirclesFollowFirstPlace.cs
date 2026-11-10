namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How long a Shaktool's middle and back attack circles last (#1592): they vanish on the first update they find the
/// first enemy-projectile place free, whichever projectile holds it, not when the front circle they trail ends. Nothing
/// that spawns the circles names the circle they follow.
/// </summary>
/// <remarks>
/// Consulted once per update by each middle and back circle, before it moves. Evidence: the trailing circles' original
/// update tests the first enemy-projectile record's identity, not the circle it was spawned with, and deletes itself
/// when that record is empty.
/// </remarks>
[CompatibilityService("a Shaktool's trailing attack circles vanish once the first enemy-projectile place is free")]
public static class ShaktoolCirclesFollowFirstPlace
{
    /// <summary>Whether a trailing circle lasts this update: the first enemy-projectile place holds a live projectile.</summary>
    /// <typeparam name="TProjectile">An enemy-projectile place.</typeparam>
    /// <param name="enemyProjectilePlaces">The room's enemy-projectile places, in place order.</param>
    /// <param name="isLive">Whether a place holds a live projectile.</param>
    /// <exception cref="ArgumentOutOfRangeException">The room has no enemy-projectile places.</exception>
    public static bool Lasts<TProjectile>(IReadOnlyList<TProjectile> enemyProjectilePlaces, Func<TProjectile, bool> isLive)
    {
        Ensure.NotNull(enemyProjectilePlaces);
        Ensure.NotNull(isLive);
        _ = Ensure.BetweenInclusive(1, int.MaxValue, enemyProjectilePlaces.Count);
        return isLive(enemyProjectilePlaces[0]);
    }
}
