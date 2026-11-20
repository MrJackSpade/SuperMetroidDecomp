namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// How a projectile moves itself on each pass (#1843, #2554): one of the motions of the behaviour that started it, bound
/// with that behaviour, which runs it (<see cref="EnemyProjectileMotionBehavior{TMotion}"/>). A motion reaches only the
/// behaviour that owns it.
/// </summary>
internal abstract class EnemyProjectileMotion
{
    /// <summary>Moves the projectile one pass by this motion, before its animation advances.</summary>
    /// <param name="context">The projectile and what its pass hands every motion.</param>
    internal abstract void Run(EnemyProjectileMotionContext context);
}
