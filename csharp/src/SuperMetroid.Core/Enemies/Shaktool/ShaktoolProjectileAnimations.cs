using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.Shaktool.ShaktoolProjectileProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// Shaktool's attack-circle programs (#641). Each
/// frame shows a <see cref="ShaktoolProjectileFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class ShaktoolProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<ShaktoolProjectileProgram, ShaktoolProjectileFrame, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<ShaktoolProjectileProgram, AnimationBeat<ShaktoolProjectileFrame, EnemyProjectileCommonCue>[]>
    {
        [P.Front] =
        [
            Show(0, 4),
            Show(1, 4),
        ],
        [P.FrontLoop] =
        [
            Show(2, 119),
        ],
        [P.Middle] =
        [
            Show(3, 6),
            Cue(new EnemyProjectileCommonCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.ShaktoolTrailingCircleFlight)),
            Show(4, 4),
        ],
        [P.MiddleLoop] =
        [
            Show(5, 119),
        ],
        [P.Back] =
        [
            Show(6, 10),
            Cue(new EnemyProjectileCommonCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.ShaktoolTrailingCircleFlight)),
        ],
        [P.BackLoop] =
        [
            Show(7, 119),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<ShaktoolProjectileProgram, Animation<ShaktoolProjectileProgram>> StartingAt { get; } = new Dictionary<ShaktoolProjectileProgram, Animation<ShaktoolProjectileProgram>>
    {
        [P.Front] = new([new(P.Front, 0, 2, 1), new(P.FrontLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.FrontLoop] = new([new(P.FrontLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Middle] = new([new(P.Middle, 0, 3, 1), new(P.MiddleLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.MiddleLoop] = new([new(P.MiddleLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Back] = new([new(P.Back, 0, 2, 1), new(P.BackLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.BackLoop] = new([new(P.BackLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<ShaktoolProjectileProgram> StartingWith(ShaktoolProjectileProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<ShaktoolProjectileFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => new AnimationBeat<ShaktoolProjectileFrame, EnemyProjectileCommonCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<ShaktoolProjectileFrame, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => new AnimationBeat<ShaktoolProjectileFrame, EnemyProjectileCommonCue>.Event(cue);
}
