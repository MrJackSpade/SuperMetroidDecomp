using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.WorkRobots.WorkRobotLaserProgram;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The Work Robot's laser programs (#641), in place of their bank-$86 lists. Generated from the verified native lists: each
/// frame shows a <see cref="WorkRobotLaserFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class WorkRobotLaserAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<WorkRobotLaserProgram, WorkRobotLaserFrame, EnemyProjectileCommonCue> Programs { get; } = new(
        new Dictionary<WorkRobotLaserProgram, AnimationProgramStep<WorkRobotLaserProgram, WorkRobotLaserFrame, EnemyProjectileCommonCue>[]>
    {
        // $86:D2EC
        [P.Initial] =
        [
            Show(0, 4),
            Show(1, 4),
            Show(2, 4),
            FallInto(P.Loop),
        ],
        // $86:D2F8
        [P.Loop] =
        [
            Show(3, 4),
            Show(4, 4),
            Show(5, 4),
            Show(6, 4),
            GoTo(P.Loop),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<WorkRobotLaserProgram, WorkRobotLaserFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => AnimationProgramStep<WorkRobotLaserProgram, WorkRobotLaserFrame, EnemyProjectileCommonCue>.Show(new(pose), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<WorkRobotLaserProgram, WorkRobotLaserFrame, EnemyProjectileCommonCue> GoTo(WorkRobotLaserProgram program) => AnimationProgramStep<WorkRobotLaserProgram, WorkRobotLaserFrame, EnemyProjectileCommonCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<WorkRobotLaserProgram, WorkRobotLaserFrame, EnemyProjectileCommonCue> FallInto(WorkRobotLaserProgram program) => AnimationProgramStep<WorkRobotLaserProgram, WorkRobotLaserFrame, EnemyProjectileCommonCue>.FallInto(program);
}
