namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The projectile definitions Work Robot fire commands spawn. The horizontal definition uses the owner's
/// signed X velocity for both directions.
/// Its values are arbitrary.
/// </summary>
internal enum WorkRobotLaser
{
    /// <summary><c>EnemyProjectile_RobotLaser_UpLeft</c>.</summary>
    UpLeft = 1,

    /// <summary><c>EnemyProjectile_RobotLaser_Horizontal</c>.</summary>
    Horizontal,

    /// <summary><c>EnemyProjectile_RobotLaser_DownLeft</c>.</summary>
    DownLeft,

    /// <summary><c>EnemyProjectile_RobotLaser_UpRight</c>.</summary>
    UpRight,

    /// <summary><c>EnemyProjectile_RobotLaser_DownRight</c>.</summary>
    DownRight,
}
