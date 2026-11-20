namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The one motion of a kind that moves itself one way (#1843): <see cref="SingleMotionProjectileBehavior"/> runs it while
/// the projectile holds it, and the projectile holds still otherwise. It has one member because such a kind has one
/// motion: the projectile's <see cref="EnemyProjectile.Motion"/> holds it or holds none, and every family's motions are an
/// enum the projectile holds, so this is that family's whole set rather than a marker (#2558).
/// </summary>
internal enum SingleMotion
{
    /// <summary>The projectile moves itself by its kind's one motion.</summary>
    Moving,
}
