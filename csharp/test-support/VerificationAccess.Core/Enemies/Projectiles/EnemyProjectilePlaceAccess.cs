namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Reaches an enemy-projectile place for checks that set up a projectile there themselves (#2572).</summary>
internal static class EnemyProjectilePlaceAccess
{
    /// <summary>
    /// The latest projectile of <paramref name="place"/>; a place the room has spawned nothing into is given a gone
    /// projectile starting from what the place's leftovers hold, as a spawn into it would, for the check to set up.
    /// </summary>
    internal static EnemyProjectile ProjectileInPlace(this EnemyProjectileEngine engine, int place) =>
        engine.LatestInPlace(place) ?? engine.Places[place].Take();

    /// <summary>
    /// The last projectile the place held, gone or not, which a check reads as the native snapshot holds a free slot: at the
    /// position and velocity its last projectile left there (#2554).
    /// </summary>
    internal static EnemyProjectile? LatestInPlace(this EnemyProjectileEngine engine, int place) =>
        PrivateState.Field<EnemyProjectilePlace, EnemyProjectile?>(engine.Places[place], "_last");

    internal static EnemyProjectile[] EveryPlace(this EnemyProjectileEngine engine) =>
        [.. Enumerable.Range(0, EnemyProjectileEngine.PlaceCount).Select(engine.ProjectileInPlace)];
}
