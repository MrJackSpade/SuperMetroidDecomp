using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.Skree.SkreeMetareeParticleProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The Skree's and Metaree's death-particle programs (#641). Each
/// frame shows a <see cref="SkreeMetareeParticleFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class SkreeMetareeParticleAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SkreeMetareeParticleProgram, SkreeMetareeParticleFrame, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<SkreeMetareeParticleProgram, AnimationBeat<SkreeMetareeParticleFrame, EnemyProjectileCommonCue>[]>
    {
        [P.Skree] =
        [
            Show(0, 16),
        ],
        [P.Metaree] =
        [
            Show(1, 16),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<SkreeMetareeParticleProgram, Animation<SkreeMetareeParticleProgram>> StartingAt { get; } = new Dictionary<SkreeMetareeParticleProgram, Animation<SkreeMetareeParticleProgram>>
    {
        [P.Skree] = new([new(P.Skree, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Metaree] = new([new(P.Metaree, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SkreeMetareeParticleProgram> StartingWith(SkreeMetareeParticleProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<SkreeMetareeParticleFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => new AnimationBeat<SkreeMetareeParticleFrame, EnemyProjectileCommonCue>.Frame(new(pose), duration);
}
