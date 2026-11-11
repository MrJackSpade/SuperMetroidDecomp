using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.WorkRobots.WorkRobotLaserProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The Work Robot's laser programs (#641). Each
/// frame shows a <see cref="WorkRobotLaserFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class WorkRobotLaserAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<WorkRobotLaserProgram, WorkRobotLaserFrame, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<WorkRobotLaserProgram, AnimationBeat<WorkRobotLaserFrame, EnemyProjectileCommonCue>[]>
    {
        [P.Initial] =
        [
            Show(0, 4),
            Show(1, 4),
            Show(2, 4),
        ],
        [P.Loop] =
        [
            Show(3, 4),
            Show(4, 4),
            Show(5, 4),
            Show(6, 4),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<WorkRobotLaserProgram, Animation<WorkRobotLaserProgram>> StartingAt { get; } = new Dictionary<WorkRobotLaserProgram, Animation<WorkRobotLaserProgram>>
    {
        [P.Initial] = new([new(P.Initial, 0, 3, 1), new(P.Loop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.Loop] = new([new(P.Loop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<WorkRobotLaserProgram> StartingWith(WorkRobotLaserProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<WorkRobotLaserFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => new AnimationBeat<WorkRobotLaserFrame, EnemyProjectileCommonCue>.Frame(new(pose), duration);
}
