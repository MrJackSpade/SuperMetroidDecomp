namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Where a Metroid that is shot or bombed recoils from (#1542): Samus's first ordinary projectile, whichever projectile
/// or bomb hit it. A free Metroid hit by a shot or a bomb flies away from that projectile's position at an eighth of
/// their distance per update, so the push follows the projectile in Samus's first shot place, or the room's origin once
/// no projectile holds it (an ended projectile reads as at rest there), not the projectile that struck.
/// </summary>
/// <remarks>
/// Consulted once per hit, by the Metroid's shot and bomb reactions after the collision prelude, before its recoil
/// velocity is stored. Evidence: the Metroid's shot reaction reads the position of projectile slot zero rather than
/// the colliding projectile's, in both the shot and the bomb collision passes.
/// </remarks>
[CompatibilityService("a shot or bombed Metroid recoils away from Samus's first ordinary projectile, whichever projectile hit it")]
public static class MetroidRecoilOrigin
{
    /// <summary>The projectile whose position a hit Metroid recoils from.</summary>
    /// <typeparam name="TProjectile">One of Samus's ordinary projectile places.</typeparam>
    /// <param name="ordinaryProjectiles">Samus's ordinary projectile places, in their order.</param>
    /// <exception cref="ArgumentOutOfRangeException">Samus has no ordinary projectile places.</exception>
    public static TProjectile Of<TProjectile>(IReadOnlyList<TProjectile> ordinaryProjectiles)
    {
        Ensure.NotNull(ordinaryProjectiles);
        _ = Ensure.BetweenInclusive(1, int.MaxValue, ordinaryProjectiles.Count);
        return ordinaryProjectiles[0];
    }
}
