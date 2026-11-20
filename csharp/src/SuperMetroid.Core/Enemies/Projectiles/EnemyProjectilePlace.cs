namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// One of the room's enemy-projectile places (#2554): free, or held by the live projectile spawned into it. A free place
/// keeps what was left in it, which the next spawn into it starts from: the whole position and velocity of the place's
/// last projectile of this room where that projectile stands, or, until the room spawns into the place, what the room
/// before left there (<see cref="EnemyProjectilePlaceLeftovers"/>).
/// </summary>
public sealed class EnemyProjectilePlace
{
    /// <summary>What the room before left in each place.</summary>
    private readonly EnemyProjectilePlaceLeftovers _leftovers;

    /// <summary>
    /// The projectile the room last spawned into the place: it holds the place while it lives, and once it is gone, or has
    /// freed the place, where it stands is what the place keeps; none until the room spawns into the place.
    /// </summary>
    private EnemyProjectile? _last;

    /// <summary>A free place keeping what the room before left in it.</summary>
    /// <param name="number">The zero-based place, zero for the last place spawns search.</param>
    /// <param name="leftovers">What the room before left in each place.</param>
    internal EnemyProjectilePlace(int number, EnemyProjectilePlaceLeftovers leftovers)
    {
        Number = Ensure.BetweenInclusive(0, EnemyProjectileEngine.PlaceCount - 1, number);
        _leftovers = Ensure.NotNull(leftovers);
    }

    /// <summary>The zero-based place, zero for the last place spawns search.</summary>
    public int Number { get; }

    /// <summary>The live projectile holding the place, or none while the place is free.</summary>
    public EnemyProjectile? Occupant => _last is { IsActive: true } live ? live : null;

    /// <summary>
    /// What the place leaves for the next projectile spawned into it: where its last projectile of this room stands, or what
    /// the room before left there while the room has spawned none into it.
    /// </summary>
    internal EnemyProjectileHandOver Left => _leftovers.StartOf(Number, LeftByLastProjectile);

    /// <summary>Where the place's last projectile of this room stands, or none while the room has spawned none into it.</summary>
    internal EnemyProjectileHandOver? LeftByLastProjectile => _last?.LeftInPlace;

    /// <summary>
    /// Spawns a fresh projectile of <paramref name="kind"/> bound to <paramref name="behavior"/> into the free place,
    /// starting from what the place keeps.
    /// </summary>
    /// <param name="kind">What the projectile is.</param>
    /// <param name="behavior">What the projectile's kind does, bound to it for its life.</param>
    internal EnemyProjectile Take(RoomEnemyProjectileKind kind, EnemyProjectileBehavior behavior) =>
        _last = new EnemyProjectile(Number, Left, kind, behavior);

    /// <summary>Frees the place for a new room: it keeps only what the room before left there.</summary>
    internal void StartRoom() => _last = null;
}
