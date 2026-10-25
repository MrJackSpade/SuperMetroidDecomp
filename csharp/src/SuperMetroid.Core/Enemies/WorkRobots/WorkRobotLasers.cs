namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>The projectile definition each Work Robot laser spawns.</summary>
internal static class WorkRobotLasers
{
    /// <summary>The projectile definition <paramref name="laser"/> spawns.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="laser"/> is not a member.</exception>
    internal static RoomEnemyProjectileKind ProjectileKind(this WorkRobotLaser laser) => laser switch
    {
        WorkRobotLaser.UpLeft => RoomEnemyProjectileKind.WorkRobotLaserUpLeft,
        WorkRobotLaser.Horizontal => RoomEnemyProjectileKind.WorkRobotLaserHorizontal,
        WorkRobotLaser.DownLeft => RoomEnemyProjectileKind.WorkRobotLaserDownLeft,
        WorkRobotLaser.UpRight => RoomEnemyProjectileKind.WorkRobotLaserUpRight,
        WorkRobotLaser.DownRight => RoomEnemyProjectileKind.WorkRobotLaserDownRight,
        _ => throw new ArgumentOutOfRangeException(nameof(laser), laser, "Undefined Work Robot laser."),
    };
}
