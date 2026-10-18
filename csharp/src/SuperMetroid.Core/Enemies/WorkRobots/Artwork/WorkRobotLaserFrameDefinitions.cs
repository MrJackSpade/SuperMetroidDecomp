using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.WorkRobots.Artwork;

/// <summary>
/// The presentation boundary of the Work Robot's laser programs (#641): the projectile frame each
/// <see cref="WorkRobotLaserFrame"/> pose shows.
/// </summary>
internal static class WorkRobotLaserFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 7;

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<WorkRobotLaserFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<WorkRobotLaserFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.WorkRobotLaserFrame00,
            [new(1)] = EnemyProjectileFrameId.WorkRobotLaserFrame01,
            [new(2)] = EnemyProjectileFrameId.WorkRobotLaserFrame02,
            [new(3)] = EnemyProjectileFrameId.WorkRobotLaserFrame03,
            [new(4)] = EnemyProjectileFrameId.WorkRobotLaserFrame04,
            [new(5)] = EnemyProjectileFrameId.WorkRobotLaserFrame05,
            [new(6)] = EnemyProjectileFrameId.WorkRobotLaserFrame06,
        };
}
