using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.Cacatac.CacatacProjectileProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// The Cacatac's spike programs, one per launch direction (#641), in place of their bank-$86 lists. Generated from the verified native lists: each
/// frame shows a <see cref="CacatacProjectileFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class CacatacProjectileAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<CacatacProjectileProgram, CacatacProjectileFrame, EnemyProjectileCommonCue> Programs { get; } = new(
        new Dictionary<CacatacProjectileProgram, AnimationProgramStep<CacatacProjectileProgram, CacatacProjectileFrame, EnemyProjectileCommonCue>[]>
    {
        [P.LeftFacingUp] =
        [
            Show(0, 1),
            Hold,
        ],
        [P.UpLeft] =
        [
            Show(1, 1),
            Hold,
        ],
        [P.Up] =
        [
            Show(2, 1),
            Hold,
        ],
        [P.UpRight] =
        [
            Show(3, 1),
            Hold,
        ],
        [P.RightFacingUp] =
        [
            Show(4, 1),
            Hold,
        ],
        [P.LeftFacingDown] =
        [
            Show(5, 1),
            Hold,
        ],
        [P.DownLeft] =
        [
            Show(6, 1),
            Hold,
        ],
        [P.Down] =
        [
            Show(7, 1),
            Hold,
        ],
        [P.DownRight] =
        [
            Show(8, 1),
            Hold,
        ],
        [P.RightFacingDown] =
        [
            Show(9, 1),
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<CacatacProjectileProgram, CacatacProjectileFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => AnimationProgramStep<CacatacProjectileProgram, CacatacProjectileFrame, EnemyProjectileCommonCue>.Show(new(pose), duration);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<CacatacProjectileProgram, CacatacProjectileFrame, EnemyProjectileCommonCue> Hold => AnimationProgramStep<CacatacProjectileProgram, CacatacProjectileFrame, EnemyProjectileCommonCue>.Hold;
}
