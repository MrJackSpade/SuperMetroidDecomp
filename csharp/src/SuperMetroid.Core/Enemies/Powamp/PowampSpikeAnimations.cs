using SuperMetroid.Core.Animation;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Enemies.Powamp.PowampSpikeSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// The Powamp spike's animations (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyProjectileCommonCue"/> with the values it acts on.
/// </summary>
internal static class PowampSpikeAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static UnpausedAnimationSegments<PowampSpikeSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<PowampSpikeSegment, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Unpaused[]>
    {
        [P.Flying] =
        [
            Show(EnemyProjectileFrameId.PowampSpikeStar, 6),
            Show(EnemyProjectileFrameId.PowampSpikeCluster, 6),
            Show(EnemyProjectileFrameId.PowampSpikeGlow, 6),
        ],
        [P.Delete] =
        [
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<PowampSpikeSegment, Animation<PowampSpikeSegment>> StartingAt { get; } = new Dictionary<PowampSpikeSegment, Animation<PowampSpikeSegment>>
    {
        [P.Flying] = Animation<PowampSpikeSegment>.Looping([Segments.Whole(P.Flying)]),
        [P.Delete] = new([Segments.Whole(P.Delete)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PowampSpikeSegment> StartingWith(PowampSpikeSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Unpaused Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Unpaused Cue(EnemyProjectileCommonCue cue) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Event(cue);
}
