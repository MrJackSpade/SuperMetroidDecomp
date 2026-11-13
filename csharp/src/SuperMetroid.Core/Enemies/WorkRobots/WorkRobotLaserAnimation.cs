using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.WorkRobots.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>Work Robot laser's typed animation.</summary>
internal sealed class WorkRobotLaserAnimation(WorkRobotLaserSegment segment)
    : EnemyProjectileFrameAnimation<WorkRobotLaserSegment, EnemyProjectileCommonCue>(WorkRobotLaserAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<WorkRobotLaserSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => WorkRobotLaserAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<WorkRobotLaserSegment, Animation<WorkRobotLaserSegment>> StartingAt => WorkRobotLaserAnimations.StartingAt;
}
