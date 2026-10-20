using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.Skree.SkreeMetareeParticleProgram;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The Skree's and Metaree's death-particle programs (#641), in place of their bank-$86 lists. Generated from the verified native lists: each
/// frame shows a <see cref="SkreeMetareeParticleFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class SkreeMetareeParticleAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<SkreeMetareeParticleProgram, SkreeMetareeParticleFrame, EnemyProjectileCommonCue> Programs { get; } = new(
        new Dictionary<SkreeMetareeParticleProgram, AnimationProgramStep<SkreeMetareeParticleProgram, SkreeMetareeParticleFrame, EnemyProjectileCommonCue>[]>
    {
        [P.Skree] =
        [
            Show(0, 16),
            GoTo(P.Skree),
        ],
        [P.Metaree] =
        [
            Show(1, 16),
            GoTo(P.Metaree),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<SkreeMetareeParticleProgram, SkreeMetareeParticleFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => AnimationProgramStep<SkreeMetareeParticleProgram, SkreeMetareeParticleFrame, EnemyProjectileCommonCue>.Show(new(pose), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<SkreeMetareeParticleProgram, SkreeMetareeParticleFrame, EnemyProjectileCommonCue> GoTo(SkreeMetareeParticleProgram program) => AnimationProgramStep<SkreeMetareeParticleProgram, SkreeMetareeParticleFrame, EnemyProjectileCommonCue>.GoTo(program);
}
