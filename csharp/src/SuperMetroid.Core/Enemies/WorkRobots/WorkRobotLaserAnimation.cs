using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.WorkRobots.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>Work Robot laser's typed animation.</summary>
internal sealed class WorkRobotLaserAnimation(WorkRobotLaserProgram program)
    : EnemyProjectileProgramAnimation<WorkRobotLaserProgram, WorkRobotLaserFrame, EnemyProjectileCommonCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<WorkRobotLaserProgram, WorkRobotLaserFrame, EnemyProjectileCommonCue> Programs => WorkRobotLaserAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(WorkRobotLaserFrame frame) =>
        WorkRobotLaserFrameDefinitions.ShownProjectileFrames[frame];
}
