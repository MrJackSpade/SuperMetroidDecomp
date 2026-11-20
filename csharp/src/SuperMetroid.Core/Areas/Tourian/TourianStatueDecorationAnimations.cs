using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using P = SuperMetroid.Core.Areas.Tourian.TourianStatueDecorationSegment;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The Tourian entrance statue's decoration animations (#641, #2554): its base decoration and the statues of Ridley and
/// Phantoon, whose cue the decoration's behaviour runs.
/// </summary>
internal static class TourianStatueDecorationAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<TourianStatueDecorationSegment, EnemyProjectileFrameId, TourianStatueDecorationCue> Segments { get; } = new(
        new Dictionary<TourianStatueDecorationSegment, AnimationBeat<EnemyProjectileFrameId, TourianStatueDecorationCue>[]>
    {
        [P.BaseDecoration] =
        [
            Show(EnemyProjectileFrameId.TourianStatueBaseDecoration, 128),
            Cue(TourianStatueDecorationCue.FollowReveal),
            Hold(EnemyProjectileFrameId.TourianStatueBaseDecoration),
        ],
        [P.Ridley] =
        [
            Hold(EnemyProjectileFrameId.TourianStatueRidley),
        ],
        [P.Phantoon] =
        [
            Hold(EnemyProjectileFrameId.TourianStatuePhantoon),
        ],
    });

    /// <summary>The animation that starts at each segment, each playing every beat of its segment (#2424).</summary>
    internal static IReadOnlyDictionary<TourianStatueDecorationSegment, Animation<TourianStatueDecorationSegment>> StartingAt { get; } = new Dictionary<TourianStatueDecorationSegment, Animation<TourianStatueDecorationSegment>>
    {
        [P.BaseDecoration] = new([Segments.Whole(P.BaseDecoration)], AnimationEnd.Holds),
        [P.Ridley] = new([Segments.Whole(P.Ridley)], AnimationEnd.Holds),
        [P.Phantoon] = new([Segments.Whole(P.Phantoon)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<TourianStatueDecorationSegment> StartingWith(TourianStatueDecorationSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TourianStatueDecorationCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, TourianStatueDecorationCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good (#2068): a statue and the base's decoration rest on their picture.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TourianStatueDecorationCue> Hold(EnemyProjectileFrameId frame) => new AnimationBeat<EnemyProjectileFrameId, TourianStatueDecorationCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TourianStatueDecorationCue> Cue(TourianStatueDecorationCue cue) => new AnimationBeat<EnemyProjectileFrameId, TourianStatueDecorationCue>.Event(cue);
}
