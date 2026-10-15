using P = SuperMetroid.Core.Enemies.Polyp.PolypRockProgram;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>
/// The Polyp's lava-rock program (#641), in place of their bank-$86 lists. Generated from the verified native lists: each
/// frame shows a <see cref="PolypRockFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class PolypRockAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<PolypRockProgram, PolypRockFrame, EnemyProjectileCommonCue> Programs { get; } = new(
        new Dictionary<PolypRockProgram, AnimationProgramStep<PolypRockProgram, PolypRockFrame, EnemyProjectileCommonCue>[]>
    {
        // $86:BBD5
        [P.Initial] =
        [
            Show(0, 1),
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<PolypRockProgram, PolypRockFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => AnimationProgramStep<PolypRockProgram, PolypRockFrame, EnemyProjectileCommonCue>.Show(new(pose), duration);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<PolypRockProgram, PolypRockFrame, EnemyProjectileCommonCue> Hold => AnimationProgramStep<PolypRockProgram, PolypRockFrame, EnemyProjectileCommonCue>.Hold;
}
