using SuperMetroid.Core.Programs;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Enemies.FallingSparks.FallingSparkProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>
/// The falling spark's programs (#641), in place of their bank-$86 lists. Generated from the verified native lists: each
/// frame shows a <see cref="FallingSparkFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class FallingSparkAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<FallingSparkProgram, FallingSparkFrame, EnemyProjectileCommonCue> Programs { get; } = new(
        new Dictionary<FallingSparkProgram, AnimationProgramStep<FallingSparkProgram, FallingSparkFrame, EnemyProjectileCommonCue>[]>
    {
        [P.Falling] =
        [
            Show(0, 3),
            Show(1, 3),
            Show(2, 3),
            GoTo(P.Falling),
        ],
        [P.HitFloor] =
        [
            Show(3, 1),
            Show(4, 1),
            Show(5, 1),
            Show(6, 1),
            Show(7, 1),
            Show(8, 1),
            Show(9, 1),
            Show(10, 1),
            Show(11, 1),
            Show(12, 1),
            Show(13, 1),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<FallingSparkProgram, FallingSparkFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => AnimationProgramStep<FallingSparkProgram, FallingSparkFrame, EnemyProjectileCommonCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<FallingSparkProgram, FallingSparkFrame, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => AnimationProgramStep<FallingSparkProgram, FallingSparkFrame, EnemyProjectileCommonCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<FallingSparkProgram, FallingSparkFrame, EnemyProjectileCommonCue> GoTo(FallingSparkProgram program) => AnimationProgramStep<FallingSparkProgram, FallingSparkFrame, EnemyProjectileCommonCue>.GoTo(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<FallingSparkProgram, FallingSparkFrame, EnemyProjectileCommonCue> Hold => AnimationProgramStep<FallingSparkProgram, FallingSparkFrame, EnemyProjectileCommonCue>.Hold;
}
