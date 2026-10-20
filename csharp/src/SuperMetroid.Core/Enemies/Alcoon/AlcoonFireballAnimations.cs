using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.Alcoon.AlcoonFireballProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>
/// Alcoon's fireball program (#641), in place of their bank-$86 lists. Generated from the verified native lists: each
/// frame shows a <see cref="AlcoonFireballFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class AlcoonFireballAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<AlcoonFireballProgram, AlcoonFireballFrame, EnemyProjectileCommonCue> Programs { get; } = new(
        new Dictionary<AlcoonFireballProgram, AnimationProgramStep<AlcoonFireballProgram, AlcoonFireballFrame, EnemyProjectileCommonCue>[]>
    {
        [P.Initial] =
        [
            Show(0, 3),
            Show(1, 3),
            Show(2, 3),
            Show(3, 3),
            GoTo(P.Initial),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<AlcoonFireballProgram, AlcoonFireballFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => AnimationProgramStep<AlcoonFireballProgram, AlcoonFireballFrame, EnemyProjectileCommonCue>.Show(new(pose), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<AlcoonFireballProgram, AlcoonFireballFrame, EnemyProjectileCommonCue> GoTo(AlcoonFireballProgram program) => AnimationProgramStep<AlcoonFireballProgram, AlcoonFireballFrame, EnemyProjectileCommonCue>.GoTo(program);
}
