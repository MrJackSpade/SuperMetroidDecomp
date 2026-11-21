namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>The projectile definition each Work Robot laser spawns.</summary>
internal static class WorkRobotLasers
{
    /// <summary>The projectile definition <paramref name="laser"/> spawns.</summary>
    internal static RoomEnemyProjectileKind ProjectileKind(this WorkRobotLaser laser) => ProjectileKinds[laser];

    /// <summary>The projectile definition each laser spawns: a diagonal laser upward or downward, a horizontal one level.</summary>
    private static IdentityTable<WorkRobotLaser, RoomEnemyProjectileKind> ProjectileKinds { get; } = new(new Dictionary<WorkRobotLaser, RoomEnemyProjectileKind>
    {
        [WorkRobotLaser.Upward] = RoomEnemyProjectileKind.WorkRobotLaserDiagonal,
        [WorkRobotLaser.Downward] = RoomEnemyProjectileKind.WorkRobotLaserDiagonal,
        [WorkRobotLaser.Horizontal] = RoomEnemyProjectileKind.WorkRobotLaserHorizontal,
    });
}
