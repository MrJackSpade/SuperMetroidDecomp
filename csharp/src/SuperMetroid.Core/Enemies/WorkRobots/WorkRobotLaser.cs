namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// Bank-$86 projectile-definition identities used by Work Robot fire commands.
/// The horizontal definition uses the owner's signed X velocity for both directions.
/// </summary>
internal enum WorkRobotLaser : ushort
{
    /// <summary><c>EnemyProjectile_RobotLaser_UpLeft</c> at $86:D2A6.</summary>
    UpLeft = 0xd2a6,

    /// <summary><c>EnemyProjectile_RobotLaser_Horizontal</c> at $86:D2B4.</summary>
    Horizontal = 0xd2b4,

    /// <summary><c>EnemyProjectile_RobotLaser_DownLeft</c> at $86:D2C2.</summary>
    DownLeft = 0xd2c2,

    /// <summary><c>EnemyProjectile_RobotLaser_UpRight</c> at $86:D2D0.</summary>
    UpRight = 0xd2d0,

    /// <summary><c>EnemyProjectile_RobotLaser_DownRight</c> at $86:D2DE.</summary>
    DownRight = 0xd2de,
}
