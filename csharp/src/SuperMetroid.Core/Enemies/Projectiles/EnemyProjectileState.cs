namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// What one projectile family keeps on a live projectile beyond the slot's shared position, motion and collision
/// fields (#1470). Each family defines its own state with named fields; the spawner gives the projectile its state and
/// the family's routines read it back as their own type.
/// </summary>
internal abstract class EnemyProjectileState;
