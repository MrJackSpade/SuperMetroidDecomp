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
    /// latest projectile left there, as a spawn into it would, for the check to set up. Every projectile runs an animation:
    /// this one deletes itself at its first animation step unless the check starts the animation it means to watch.
    /// </summary>
    internal static EnemyProjectile Occupy(this EnemyProjectileEngine engine, int place, RoomEnemyProjectileKind kind) =>
        engine.Occupy(place, kind, EnemyProjectileDeleteAnimation.Instance);

    /// <summary>
    /// Makes <paramref name="place"/> hold a fresh projectile of <paramref name="kind"/> running <paramref name="animation"/>,
    /// starting from what the place's latest projectile left there, as a spawn into it would.
    /// </summary>
    internal static EnemyProjectile Occupy(this EnemyProjectileEngine engine, int place, RoomEnemyProjectileKind kind, EnemyProjectileAnimation animation)
    {
        EnemyProjectile?[] places = engine.ProjectilesInPlaces;
        EnemyProjectilePlaceLeftovers leftovers = engine.Leftovers;
        return places[place] = new EnemyProjectile(place, leftovers.StartOf(place, places[place]?.LeftInPlace), kind, animation);
    }

    /// <summary>
    /// Makes <paramref name="place"/> free, holding <paramref name="leftover"/> for the room's next spawn there, as a
    /// recorded state's free place holds what its last projectile left: no projectile holds the place.
    /// </summary>
    internal static void ImportLeftover(this EnemyProjectileEngine engine, int place, EnemyProjectileHandOver leftover)
    {
        EnemyProjectile?[] places = engine.ProjectilesInPlaces;
        EnemyProjectilePlaceLeftovers leftovers = engine.Leftovers;
        leftovers.FromRoomBefore[place] = leftover;
        places[place] = null;
    }

    /// <summary>Every place's latest projectile in place order, for checks that fill the pool.</summary>
    /// <exception cref="InvalidOperationException">The room has spawned nothing into some place.</exception>
    internal static EnemyProjectile[] EveryPlace(this EnemyProjectileEngine engine) =>
        [.. Enumerable.Range(0, EnemyProjectilePlaces.Capacity).Select(engine.SpawnedInPlace)];
}
