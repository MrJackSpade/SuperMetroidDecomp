using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>Work Robot laser's typed animation.</summary>
internal sealed partial class WorkRobotLaserAnimation
    : EnemyProjectileFrameAnimation<WorkRobotLaserSegment, EnemyProjectileCommonCue>
{
    /// <summary>Creates a <see cref="WorkRobotLaserAnimation"/>.</summary>
    /// <param name="segment">segment</param>
    public WorkRobotLaserAnimation(WorkRobotLaserSegment segment)
        : base(segment, WorkRobotLaserAnimations.StartingAt[segment], EnemyProjectileCueRunners.Common<WorkRobotLaserSegment>())
    {
    }

    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<WorkRobotLaserSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => WorkRobotLaserAnimations.Segments;
}
