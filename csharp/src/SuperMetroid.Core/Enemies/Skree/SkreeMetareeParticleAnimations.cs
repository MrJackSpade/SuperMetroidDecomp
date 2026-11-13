using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.Skree.SkreeMetareeParticleSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The Skree's and Metaree's death-particle programs (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class SkreeMetareeParticleAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SkreeMetareeParticleSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<SkreeMetareeParticleSegment, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.Skree] =
        [
            Show(EnemyProjectileFrameId.SkreeDebris, 16),
        ],
        [P.Metaree] =
        [
            Show(EnemyProjectileFrameId.MetareeDebris, 16),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<SkreeMetareeParticleSegment, Animation<SkreeMetareeParticleSegment>> StartingAt { get; } = new Dictionary<SkreeMetareeParticleSegment, Animation<SkreeMetareeParticleSegment>>
    {
        [P.Skree] = new([new(P.Skree, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Metaree] = new([new(P.Metaree, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SkreeMetareeParticleSegment> StartingWith(SkreeMetareeParticleSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);
}
