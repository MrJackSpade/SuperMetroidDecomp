using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>Work Robot laser's typed animation.</summary>
internal sealed class WorkRobotLaserAnimation(WorkRobotLaserSegment segment)
    : EnemyProjectileFrameAnimation<WorkRobotLaserSegment, EnemyProjectileCommonCue>(segment, WorkRobotLaserAnimations.StartingAt[segment], EnemyProjectileCueRunners.Common<WorkRobotLaserSegment>())
{
    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<WorkRobotLaserSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => WorkRobotLaserAnimations.Segments;
}
