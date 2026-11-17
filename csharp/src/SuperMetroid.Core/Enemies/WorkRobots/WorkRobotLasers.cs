namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>The projectile definition each Work Robot laser spawns.</summary>
internal static class WorkRobotLasers
{
    /// <summary>The projectile definition <paramref name="laser"/> spawns.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="laser"/> is not a member.</exception>
    internal static RoomEnemyProjectileKind ProjectileKind(this WorkRobotLaser laser) => laser switch
    {
        WorkRobotLaser.Upward or WorkRobotLaser.Downward => RoomEnemyProjectileKind.WorkRobotLaserDiagonal,
        WorkRobotLaser.Horizontal => RoomEnemyProjectileKind.WorkRobotLaserHorizontal,
        _ => throw new ArgumentOutOfRangeException(nameof(laser), laser, "Undefined Work Robot laser."),
    };
}
