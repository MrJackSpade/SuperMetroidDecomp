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
    internal static HidingUnpausedAnimationSegments<FallingSparkSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<FallingSparkSegment, IUnpausedHidingAnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.Falling] =
        [
            Show(EnemyProjectileFrameId.FallingSparkUpright, 3),
            Show(EnemyProjectileFrameId.FallingSparkDiagonal, 3),
            Show(EnemyProjectileFrameId.FallingSparkUprightSmall, 3),
        ],
        [P.HitFloor] =
        [
            .. Flickering(EnemyProjectileFrameId.FallingSparkUpright, FloorFlickers),
            .. Flickering(EnemyProjectileFrameId.FallingSparkDiagonal, FloorFlickers),
            .. Flickering(EnemyProjectileFrameId.FallingSparkUprightSmall, 1),
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

    /// <summary>
    /// How many times a spark that hits the floor flickers in each of its upright and diagonal shapes, before it flickers
    /// once in its small shape and shows that shape once more as it goes.
    /// </summary>
    private const int FloorFlickers = 2;

    /// <summary>
    /// A spark flickering <paramref name="times"/> times (#2432): <paramref name="frame"/> for an update, then nothing for an
    /// update.
    /// </summary>
    private static IEnumerable<IUnpausedHidingAnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>> Flickering(EnemyProjectileFrameId frame, int times) =>
        Enumerable.Repeat<IUnpausedHidingAnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>([Show(frame, 1), Hide(1)], times)
            .SelectMany(flicker => flicker);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Unpaused Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);

    /// <summary>Shows nothing for <paramref name="duration"/> updates (#2558): the projectile is hidden until its next frame.</summary>
    private static AnimationHiddenBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Hide(int duration) =>
        new AnimationHiddenBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>(duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Unpaused Cue(EnemyProjectileCommonCue cue) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Event(cue);
}
