using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.Stoke.StokeProjectileProgram;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>
/// The Stoke's projectile program (#641), in place of their bank-$86 lists. Generated from the verified native lists: each
/// frame shows a <see cref="StokeProjectileFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class StokeProjectileAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<StokeProjectileProgram, StokeProjectileFrame, EnemyProjectileCommonCue> Programs { get; } = new(
        new Dictionary<StokeProjectileProgram, AnimationProgramStep<StokeProjectileProgram, StokeProjectileFrame, EnemyProjectileCommonCue>[]>
    {
        [P.Initial] =
        [
            Show(0, 16),
            Show(1, 16),
            GoTo(P.Initial),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<StokeProjectileProgram, StokeProjectileFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => AnimationProgramStep<StokeProjectileProgram, StokeProjectileFrame, EnemyProjectileCommonCue>.Show(new(pose), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<StokeProjectileProgram, StokeProjectileFrame, EnemyProjectileCommonCue> GoTo(StokeProjectileProgram program) => AnimationProgramStep<StokeProjectileProgram, StokeProjectileFrame, EnemyProjectileCommonCue>.GoTo(program);
}
