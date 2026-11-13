using SuperMetroid.Core.Programs;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Enemies.FallingSparks.FallingSparkSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>
/// The falling spark's animations (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class FallingSparkAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<FallingSparkSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<FallingSparkSegment, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.Falling] =
        [
            Show(EnemyProjectileFrameId.FallingSparkFallingFirst, 3),
            Show(EnemyProjectileFrameId.FallingSparkFallingSecond, 3),
            Show(EnemyProjectileFrameId.FallingSparkFallingThird, 3),
        ],
        [P.HitFloor] =
        [
            Show(EnemyProjectileFrameId.FallingSparkHitFloorFirst, 1),
            Show(EnemyProjectileFrameId.FallingSparkHitFloorSecond, 1),
            Show(EnemyProjectileFrameId.FallingSparkHitFloorThird, 1),
            Show(EnemyProjectileFrameId.FallingSparkHitFloorFourth, 1),
            Show(EnemyProjectileFrameId.FallingSparkHitFloorFifth, 1),
            Show(EnemyProjectileFrameId.FallingSparkHitFloorSixth, 1),
            Show(EnemyProjectileFrameId.FallingSparkHitFloorSeventh, 1),
            Show(EnemyProjectileFrameId.FallingSparkHitFloorEighth, 1),
            Show(EnemyProjectileFrameId.FallingSparkHitFloorNinth, 1),
            Show(EnemyProjectileFrameId.FallingSparkHitFloorTenth, 1),
            Show(EnemyProjectileFrameId.FallingSparkHitFloorEleventh, 1),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<FallingSparkSegment, Animation<FallingSparkSegment>> StartingAt { get; } = new Dictionary<FallingSparkSegment, Animation<FallingSparkSegment>>
    {
        [P.Falling] = new([new(P.Falling, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HitFloor] = new([new(P.HitFloor, 0, 12, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<FallingSparkSegment> StartingWith(FallingSparkSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Event(cue);
}
