using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.Cacatac.CacatacProjectileProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// The Cacatac's spike programs, one per launch direction (#641). Each
/// frame shows a <see cref="CacatacProjectileFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class CacatacProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<CacatacProjectileProgram, CacatacProjectileFrame, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<CacatacProjectileProgram, AnimationBeat<CacatacProjectileFrame, EnemyProjectileCommonCue>[]>
    {
        [P.LeftFacingUp] =
        [
            Show(0, 1),
        ],
        [P.UpLeft] =
        [
            Show(1, 1),
        ],
        [P.Up] =
        [
            Show(2, 1),
        ],
        [P.UpRight] =
        [
            Show(3, 1),
        ],
        [P.RightFacingUp] =
        [
            Show(4, 1),
        ],
        [P.LeftFacingDown] =
        [
            Show(5, 1),
        ],
        [P.DownLeft] =
        [
            Show(6, 1),
        ],
        [P.Down] =
        [
            Show(7, 1),
        ],
        [P.DownRight] =
        [
            Show(8, 1),
        ],
        [P.RightFacingDown] =
        [
            Show(9, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<CacatacProjectileProgram, Animation<CacatacProjectileProgram>> StartingAt { get; } = new Dictionary<CacatacProjectileProgram, Animation<CacatacProjectileProgram>>
    {
        [P.LeftFacingUp] = new([new(P.LeftFacingUp, 0, 1, 1)], AnimationEnd.Holds),
        [P.UpLeft] = new([new(P.UpLeft, 0, 1, 1)], AnimationEnd.Holds),
        [P.Up] = new([new(P.Up, 0, 1, 1)], AnimationEnd.Holds),
        [P.UpRight] = new([new(P.UpRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.RightFacingUp] = new([new(P.RightFacingUp, 0, 1, 1)], AnimationEnd.Holds),
        [P.LeftFacingDown] = new([new(P.LeftFacingDown, 0, 1, 1)], AnimationEnd.Holds),
        [P.DownLeft] = new([new(P.DownLeft, 0, 1, 1)], AnimationEnd.Holds),
        [P.Down] = new([new(P.Down, 0, 1, 1)], AnimationEnd.Holds),
        [P.DownRight] = new([new(P.DownRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.RightFacingDown] = new([new(P.RightFacingDown, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CacatacProjectileProgram> StartingWith(CacatacProjectileProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<CacatacProjectileFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => new AnimationBeat<CacatacProjectileFrame, EnemyProjectileCommonCue>.Frame(new(pose), duration);
}
