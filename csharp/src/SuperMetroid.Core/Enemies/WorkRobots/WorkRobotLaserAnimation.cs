using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>Work Robot laser's typed animation.</summary>
internal sealed class WorkRobotLaserAnimation(WorkRobotLaserSegment segment)
    : EnemyProjectileFrameAnimation<WorkRobotLaserSegment, EnemyProjectileCommonCue>(WorkRobotLaserAnimations.StartingWith(segment), EnemyProjectileCueRunners.Common<WorkRobotLaserSegment>())
{
    /// <inheritdoc/>
    private protected override UnpausedAnimationSegments<WorkRobotLaserSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => WorkRobotLaserAnimations.Segments;

    /// <inheritdoc/>
    private protected override IEnumerable<KeyValuePair<WorkRobotLaserSegment, Animation<WorkRobotLaserSegment>>> StartingAt => WorkRobotLaserAnimations.StartingAt;
}
