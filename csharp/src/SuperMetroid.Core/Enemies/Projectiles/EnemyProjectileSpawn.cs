namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile just spawned with the behaviour created for it (#2554): its spawner reads the behaviour's state, which holds
/// the projectile it shows.
/// </summary>
/// <typeparam name="TBehavior">The behaviour's type.</typeparam>
/// <param name="Projectile">The spawned projectile.</param>
/// <param name="Behavior">The behaviour bound to it.</param>
internal sealed record EnemyProjectileSpawn<TBehavior>(EnemyProjectile Projectile, TBehavior Behavior)
    where TBehavior : EnemyProjectileBehavior;
