using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// What a projectile that stays bound to the Torizo that made it keeps (#2030): Golden Torizo's eye beam, whose floor
/// sparks spark again while its eye keeps their explosions disabled, and the Super Missile its hand catches and holds.
/// </summary>
/// <param name="owner">The place of the Torizo that fired or caught the projectile.</param>
/// <param name="torizo">The state of the Torizo that fired or caught the projectile.</param>
internal sealed class TorizoOwnedProjectileState(RoomEnemyPlace owner, TorizoEnemyState torizo) : EnemyProjectileState
{
    /// <summary>The place of the Torizo that fired or caught the projectile.</summary>
    internal RoomEnemyPlace Owner { get; } = Ensure.NotNull(owner);

    /// <summary>The state of the Torizo that fired or caught the projectile, which the projectile reads its facing and eye from.</summary>
    internal TorizoEnemyState Torizo { get; } = Ensure.NotNull(torizo);
}
