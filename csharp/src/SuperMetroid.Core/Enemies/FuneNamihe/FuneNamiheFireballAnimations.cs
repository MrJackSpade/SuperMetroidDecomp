using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.FuneNamihe.FuneNamiheFireballProgram;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// Fune and Namihe's fireball programs (#641), in place of their bank-$86 lists. Generated from the verified native lists: each
/// frame shows a <see cref="FuneNamiheFireballFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class FuneNamiheFireballAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<FuneNamiheFireballProgram, FuneNamiheFireballFrame, EnemyProjectileCommonCue> Programs { get; } = new(
        new Dictionary<FuneNamiheFireballProgram, AnimationProgramStep<FuneNamiheFireballProgram, FuneNamiheFireballFrame, EnemyProjectileCommonCue>[]>
    {
        [P.Left] =
        [
            Show(0, 5),
            Show(1, 5),
            Show(2, 5),
            GoTo(P.Left),
        ],
        [P.Right] =
        [
            Show(3, 5),
            Show(4, 5),
            Show(5, 5),
            GoTo(P.Right),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<FuneNamiheFireballProgram, FuneNamiheFireballFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => AnimationProgramStep<FuneNamiheFireballProgram, FuneNamiheFireballFrame, EnemyProjectileCommonCue>.Show(new(pose), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<FuneNamiheFireballProgram, FuneNamiheFireballFrame, EnemyProjectileCommonCue> GoTo(FuneNamiheFireballProgram program) => AnimationProgramStep<FuneNamiheFireballProgram, FuneNamiheFireballFrame, EnemyProjectileCommonCue>.GoTo(program);
}
