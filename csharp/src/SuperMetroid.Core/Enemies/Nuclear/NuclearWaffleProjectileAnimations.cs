using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.Nuclear.NuclearWaffleProjectileProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>
/// The Nuclear Waffle's body program (#641), in place of their bank-$86 lists. Generated from the verified native lists: each
/// frame shows a <see cref="NuclearWaffleProjectileFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class NuclearWaffleProjectileAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<NuclearWaffleProjectileProgram, NuclearWaffleProjectileFrame, EnemyProjectileCommonCue> Programs { get; } = new(
        new Dictionary<NuclearWaffleProjectileProgram, AnimationProgramStep<NuclearWaffleProjectileProgram, NuclearWaffleProjectileFrame, EnemyProjectileCommonCue>[]>
    {
        [P.Initial] =
        [
            Show(0, 3),
            Show(1, 3),
            Show(2, 3),
            Show(3, 3),
            Show(4, 3),
            Show(5, 3),
            Show(6, 3),
            Show(7, 3),
            Show(8, 3),
            Show(9, 3),
            Show(10, 3),
            Show(11, 3),
            GoTo(P.Initial),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<NuclearWaffleProjectileProgram, NuclearWaffleProjectileFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => AnimationProgramStep<NuclearWaffleProjectileProgram, NuclearWaffleProjectileFrame, EnemyProjectileCommonCue>.Show(new(pose), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<NuclearWaffleProjectileProgram, NuclearWaffleProjectileFrame, EnemyProjectileCommonCue> GoTo(NuclearWaffleProjectileProgram program) => AnimationProgramStep<NuclearWaffleProjectileProgram, NuclearWaffleProjectileFrame, EnemyProjectileCommonCue>.GoTo(program);
}
