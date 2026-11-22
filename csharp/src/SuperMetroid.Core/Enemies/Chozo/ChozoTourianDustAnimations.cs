using SuperMetroid.Core.Animation;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Enemies.Chozo.ChozoTourianDustSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The Wrecked Ship Chozo's footstep dust and the Tourian statue's descent dust (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyProjectileCommonCue"/> with the values it acts on.
/// </summary>
internal static partial class ChozoTourianDustAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static UnpausedAnimationSegments<ChozoTourianDustSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<ChozoTourianDustSegment, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Unpaused[]>
    {
        [P.Footsteps] =
        [
            Cue(new EnemyProjectileCommonCue.Scatter(Spread: new(Span: new(16, 16), Center: new(0, 3)))),
            .. PuffToCloud,
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.TourianDescentDust] =
        [
            Cue(new EnemyProjectileCommonCue.Signal(CK.ReturnToOrigin)),
            Cue(new EnemyProjectileCommonCue.Scatter(Spread: new(Span: new(64, 4), Center: new(0, 0)))),
            .. PuffToCloud,
        ],
        [P.TourianDescentDustEnd] =
        [
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
    });

    /// <summary>The dust's puff growing to a dense cloud, two updates a picture, which both the footsteps and the descent raise.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Unpaused[] PuffToCloud =>
    [
        Show(EnemyProjectileFrameId.ChozoDustPuff, 2),
        Show(EnemyProjectileFrameId.ChozoDustPuffPair, 2),
        Show(EnemyProjectileFrameId.ChozoDustPuffTrio, 2),
        Show(EnemyProjectileFrameId.ChozoDustCloudsDense, 2),
    ];

    /// <summary>How many puffs the Tourian statue's descent dust raises before it is deleted.</summary>
    private const int DescentDustPuffs = 64;

    /// <summary>The animation that starts at each segment an owner can start or restart at: the footstep dust's.</summary>
    internal static IReadOnlyDictionary<ChozoTourianDustSegment, Animation<ChozoTourianDustSegment>> StartingAt { get; } = new Dictionary<ChozoTourianDustSegment, Animation<ChozoTourianDustSegment>>
    {
        [P.Footsteps] = new([Segments.Whole(P.Footsteps)], AnimationEnd.Holds),
    };

    /// <summary>
    /// The dust the Tourian statue raises as it sinks (#2061): sixty-four puffs scattered about its origin, then deleted.
    /// </summary>
    internal static Animation<ChozoTourianDustSegment> TourianDescentDust { get; } =
        new([Segments.Repeated(P.TourianDescentDust, DescentDustPuffs), Segments.Whole(P.TourianDescentDustEnd)], AnimationEnd.Holds);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Unpaused Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Unpaused Cue(EnemyProjectileCommonCue cue) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Event(cue);
}
