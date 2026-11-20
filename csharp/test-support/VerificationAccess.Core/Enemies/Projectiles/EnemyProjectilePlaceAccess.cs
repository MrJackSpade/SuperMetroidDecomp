namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Reaches an enemy-projectile place for checks that set up a projectile there themselves (#2572).</summary>
internal static class EnemyProjectilePlaceAccess
{
    /// <summary>
    /// The latest projectile of <paramref name="place"/>; a place the room has spawned nothing into is given a gone dust
    /// cloud starting from what the place's leftovers hold, as a spawn into it would, for the check to set up.
    /// </summary>
    internal static EnemyProjectile ProjectileInPlace(this EnemyProjectileEngine engine, int place)
    {
        if (engine.LatestInPlace(place) is { } latest)
            return latest;
        EnemyProjectile?[] places = PrivateState.Field<EnemyProjectileEngine, EnemyProjectile?[]>(engine, "_places");
        EnemyProjectilePlaceLeftovers leftovers = PrivateState.Field<EnemyProjectileEngine, EnemyProjectilePlaceLeftovers>(engine, "_leftovers");
        var gone = new EnemyProjectile(place, leftovers.StartOf(place, null), RoomEnemyProjectileKind.DustOrExplosion);
        gone.Delete();
        return places[place] = gone;
    }

    /// <summary>
    /// Makes <paramref name="place"/> hold a fresh projectile of <paramref name="kind"/>, starting from what the place's
    /// latest projectile left there, as a spawn into it would, for the check to set up.
    /// </summary>
    internal static EnemyProjectile Occupy(this EnemyProjectileEngine engine, int place, RoomEnemyProjectileKind kind)
    {
        EnemyProjectile?[] places = PrivateState.Field<EnemyProjectileEngine, EnemyProjectile?[]>(engine, "_places");
        EnemyProjectilePlaceLeftovers leftovers = PrivateState.Field<EnemyProjectileEngine, EnemyProjectilePlaceLeftovers>(engine, "_leftovers");
        return places[place] = new EnemyProjectile(place, leftovers.StartOf(place, places[place]?.LeftInPlace), kind);
    }

    /// <summary>Every place's projectile in place order, as <see cref="ProjectileInPlace"/> gives each, for checks that fill the pool.</summary>
    internal static EnemyProjectile[] EveryPlace(this EnemyProjectileEngine engine) =>
        [.. Enumerable.Range(0, EnemyProjectileEngine.PlaceCount).Select(engine.ProjectileInPlace)];
}
