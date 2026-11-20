using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>Work Robot laser's typed animation.</summary>
internal sealed class WorkRobotLaserAnimation(WorkRobotLaserSegment segment)
    : EnemyProjectileCommonFrameAnimation<WorkRobotLaserSegment>(WorkRobotLaserAnimations.StartingWith(segment), EnemyProjectileCueRunners.Common<WorkRobotLaserSegment>())
{
    /// <inheritdoc/>
    private protected override AnimationSegments<WorkRobotLaserSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => WorkRobotLaserAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<WorkRobotLaserSegment, Animation<WorkRobotLaserSegment>> StartingAt => WorkRobotLaserAnimations.StartingAt;
}
