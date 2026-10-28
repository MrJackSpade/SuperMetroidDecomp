namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The projectile definitions Work Robot fire commands spawn. The horizontal definition uses the owner's
/// signed X velocity for both directions.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
internal enum WorkRobotLaser
{
    /// <summary><c>EnemyProjectile_RobotLaser_UpLeft</c> at $86:D2A6.</summary>
    UpLeft = 1,

    /// <summary><c>EnemyProjectile_RobotLaser_Horizontal</c> at $86:D2B4.</summary>
    Horizontal,

    /// <summary><c>EnemyProjectile_RobotLaser_DownLeft</c> at $86:D2C2.</summary>
    DownLeft,

    /// <summary><c>EnemyProjectile_RobotLaser_UpRight</c> at $86:D2D0.</summary>
    UpRight,

    /// <summary><c>EnemyProjectile_RobotLaser_DownRight</c> at $86:D2DE.</summary>
    DownRight,
}
