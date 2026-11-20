namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Reaches an enemy-projectile place for checks that set up a projectile there themselves (#2572).</summary>
internal static class EnemyProjectilePlaceAccess
{
    /// <summary>The latest projectile the room spawned into <paramref name="place"/>, live or gone, which a check reads.</summary>
    /// <exception cref="InvalidOperationException">The room has spawned nothing into the place.</exception>
    internal static EnemyProjectile SpawnedInPlace(this EnemyProjectileEngine engine, int place) =>
        engine.LatestInPlace(place) ?? throw new InvalidOperationException(
            $"A check reads enemy-projectile place {place}, which the room has spawned nothing into.");

    /// <summary>
    /// Makes <paramref name="place"/> hold a fresh projectile of <paramref name="kind"/>, starting from what the place's
    /// latest projectile left there, as a spawn into it would, for the check to set up.
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
        [.. Enumerable.Range(0, EnemyProjectileEngine.PlaceCount).Select(engine.SpawnedInPlace)];
}
