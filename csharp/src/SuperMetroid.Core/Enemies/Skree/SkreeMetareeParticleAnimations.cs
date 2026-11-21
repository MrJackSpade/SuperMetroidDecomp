using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Enemies.Skree.SkreeMetareeParticleSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The Skree's and Metaree's death-particle animations (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyProjectileCommonCue"/> with the values it acts on.
/// </summary>
internal static class SkreeMetareeParticleAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static UnpausedAnimationSegments<SkreeMetareeParticleSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<SkreeMetareeParticleSegment, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Unpaused[]>
    {
        [P.Skree] =
        [
            Hold(EnemyProjectileFrameId.SkreeDebris),
        ],
        [P.Metaree] =
        [
            Hold(EnemyProjectileFrameId.MetareeDebris),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<SkreeMetareeParticleSegment, Animation<SkreeMetareeParticleSegment>> StartingAt { get; } = new Dictionary<SkreeMetareeParticleSegment, Animation<SkreeMetareeParticleSegment>>
    {
        [P.Skree] = new([Segments.Whole(P.Skree)], AnimationEnd.Holds),
        [P.Metaree] = new([Segments.Whole(P.Metaree)], AnimationEnd.Holds),
    };

    /// <summary>Shows <paramref name="frame"/> for good (#2276): a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Unpaused Hold(EnemyProjectileFrameId frame) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.HeldFrame(frame);
}
