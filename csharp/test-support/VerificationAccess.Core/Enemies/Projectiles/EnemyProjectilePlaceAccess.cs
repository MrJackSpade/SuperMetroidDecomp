using SuperMetroid.Core.World;

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
<<<<<<< ours
    /// Makes <paramref name="place"/> of <paramref name="enemies"/>' projectiles hold a fresh projectile of
    /// <paramref name="kind"/> bound to the behaviour its production spawner binds and drawing with
    /// <paramref name="graphics"/> (#2554), copying its kind's definition as a spawn into the place does. Only the snapshot
    /// import uses it, to restore a recorded place, until it writes the pool through the generated state-graph restore
    /// (#2602); checks spawn through the game (<see cref="FamilyProjectileStarts.Allocate"/>).
||||||| base
    /// Makes <paramref name="place"/> hold a fresh projectile of <paramref name="kind"/>, starting from what the place's
    /// latest projectile left there, as a spawn into it would, for the check to set up.
=======
    /// Makes <paramref name="place"/> hold a fresh projectile of <paramref name="kind"/>, starting from what the place's
    /// latest projectile left there, as a spawn into it would, for the check to set up. Every projectile runs an animation:
    /// this one deletes itself at its first animation step unless the check starts the animation it means to watch.
>>>>>>> theirs
    /// </summary>
<<<<<<< ours
    internal static EnemyProjectile OccupyProjectile(this RoomEnemySystem enemies, int place, RoomEnemyProjectileKind kind, SpriteGraphics graphics) =>
        Ensure.NotNull(enemies).Projectiles.Occupy(place, kind, FamilyProjectileStarts.BehaviorOf(enemies, kind), graphics);
||||||| base
    internal static EnemyProjectile Occupy(this EnemyProjectileEngine engine, int place, RoomEnemyProjectileKind kind)
    {
        EnemyProjectile?[] places = PrivateState.Field<EnemyProjectileEngine, EnemyProjectile?[]>(engine, "_places");
        EnemyProjectilePlaceLeftovers leftovers = PrivateState.Field<EnemyProjectileEngine, EnemyProjectilePlaceLeftovers>(engine, "_leftovers");
        return places[place] = new EnemyProjectile(place, leftovers.StartOf(place, places[place]?.LeftInPlace), kind);
    }
=======
    internal static EnemyProjectile Occupy(this EnemyProjectileEngine engine, int place, RoomEnemyProjectileKind kind) =>
        engine.Occupy(place, kind, EnemyProjectileDeleteAnimation.Instance);

    /// <summary>
    /// Makes <paramref name="place"/> hold a fresh projectile of <paramref name="kind"/> running <paramref name="animation"/>,
    /// starting from what the place's latest projectile left there, as a spawn into it would.
    /// </summary>
    internal static EnemyProjectile Occupy(this EnemyProjectileEngine engine, int place, RoomEnemyProjectileKind kind, EnemyProjectileAnimation animation)
    {
        EnemyProjectile?[] places = PrivateState.Field<EnemyProjectileEngine, EnemyProjectile?[]>(engine, "_places");
        EnemyProjectilePlaceLeftovers leftovers = PrivateState.Field<EnemyProjectileEngine, EnemyProjectilePlaceLeftovers>(engine, "_leftovers");
        return places[place] = new EnemyProjectile(place, leftovers.StartOf(place, places[place]?.LeftInPlace), kind, animation);
    }
>>>>>>> theirs

    /// <summary>
    /// Makes <paramref name="place"/> free, holding <paramref name="leftover"/> for the room's next spawn there, as a
    /// recorded state's free place holds what its last projectile left: no projectile holds the place.
    /// </summary>
    internal static void ImportLeftover(this EnemyProjectileEngine engine, int place, EnemyProjectileHandOver leftover)
    {
        EnemyProjectile?[] places = PrivateState.Field<EnemyProjectileEngine, EnemyProjectile?[]>(engine, "_places");
        EnemyProjectilePlaceLeftovers leftovers = PrivateState.Field<EnemyProjectileEngine, EnemyProjectilePlaceLeftovers>(engine, "_leftovers");
        PrivateState.Field<EnemyProjectilePlaceLeftovers, EnemyProjectileHandOver[]>(leftovers, "_fromRoomBefore")[place] = leftover;
        places[place] = null;
    }

    /// <summary>Every place's latest projectile in place order, for checks that fill the pool.</summary>
    /// <exception cref="InvalidOperationException">The room has spawned nothing into some place.</exception>
    internal static EnemyProjectile[] EveryPlace(this EnemyProjectileEngine engine) =>
        [.. Enumerable.Range(0, EnemyProjectilePlaces.Capacity).Select(engine.SpawnedInPlace)];
}
