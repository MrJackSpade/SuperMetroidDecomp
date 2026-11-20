namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// How a projectile moves itself on each pass (#1843, #2571): one motion of the behaviour bound to it, held through that
/// behaviour, so the projectile never holds a motion its behaviour does not own and no pass tests a motion's type.
/// </summary>
internal abstract class EnemyProjectileMotion
{
    /// <summary>Only a behaviour's typed motion derives from it.</summary>
    private protected EnemyProjectileMotion()
    {
    }

    /// <summary>Moves the projectile one pass by this motion, before its animation advances.</summary>
    /// <param name="context">The projectile and what its pass hands every motion.</param>
    internal abstract void Move(EnemyProjectileMotionContext context);
}
