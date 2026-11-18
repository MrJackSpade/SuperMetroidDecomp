using SuperMetroid.Core.Animation;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Enemies.FallingSparks.FallingSparkSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>
/// The falling spark's animations (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyProjectileCommonCue"/> with the values it acts on.
/// </summary>
internal static class FallingSparkAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<FallingSparkSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<FallingSparkSegment, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.Falling] =
        [
            Show(EnemyProjectileFrameId.FallingSparkUpright, 3),
            Show(EnemyProjectileFrameId.FallingSparkDiagonal, 3),
            Show(EnemyProjectileFrameId.FallingSparkUprightSmall, 3),
        ],
        [P.HitFloor] =
        [
            Show(EnemyProjectileFrameId.FallingSparkUpright, 1),
            Show(EnemyProjectileFrameId.FallingSparkBlank, 1),
            Show(EnemyProjectileFrameId.FallingSparkUpright, 1),
            Show(EnemyProjectileFrameId.FallingSparkBlank, 1),
            Show(EnemyProjectileFrameId.FallingSparkDiagonal, 1),
            Show(EnemyProjectileFrameId.FallingSparkBlank, 1),
            Show(EnemyProjectileFrameId.FallingSparkDiagonal, 1),
            Show(EnemyProjectileFrameId.FallingSparkBlank, 1),
            Show(EnemyProjectileFrameId.FallingSparkUprightSmall, 1),
            Show(EnemyProjectileFrameId.FallingSparkBlank, 1),
            Show(EnemyProjectileFrameId.FallingSparkUprightSmall, 1),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<FallingSparkSegment, Animation<FallingSparkSegment>> StartingAt { get; } = new Dictionary<FallingSparkSegment, Animation<FallingSparkSegment>>
    {
        [P.Falling] = Animation<FallingSparkSegment>.Looping([Segments.Whole(P.Falling)]),
        [P.HitFloor] = new([Segments.Whole(P.HitFloor)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<FallingSparkSegment> StartingWith(FallingSparkSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Event(cue);
}
