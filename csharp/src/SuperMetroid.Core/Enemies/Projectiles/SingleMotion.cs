namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The one motion of a kind that moves itself one way (#1843): <see cref="SingleMotionProjectileBehavior"/> runs it while
/// the projectile holds it, and the projectile holds still otherwise.
/// </summary>
internal enum SingleMotion
{
    /// <summary>The projectile moves itself by its kind's one motion.</summary>
    Moving,
}
