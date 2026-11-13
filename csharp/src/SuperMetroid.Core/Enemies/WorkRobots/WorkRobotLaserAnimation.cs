using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.WorkRobots.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>Work Robot laser's typed animation.</summary>
internal sealed class WorkRobotLaserAnimation(WorkRobotLaserProgram program)
    : EnemyProjectileFrameAnimation<WorkRobotLaserProgram, EnemyProjectileCommonCue>(WorkRobotLaserAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<WorkRobotLaserProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => WorkRobotLaserAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<WorkRobotLaserProgram, Animation<WorkRobotLaserProgram>> StartingAt => WorkRobotLaserAnimations.StartingAt;
}
