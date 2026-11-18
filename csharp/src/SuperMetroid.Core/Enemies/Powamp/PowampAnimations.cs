using P = SuperMetroid.Core.Enemies.Powamp.PowampSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// Powamp's body pulses and balloon inflate and sink animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class PowampAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<PowampSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<PowampSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.BodyFast] =
        [
            Show(EnemyFrameId.PowampBodyTailLeft, 5),
            Show(EnemyFrameId.PowampBodyTailDown, 5),
            Show(EnemyFrameId.PowampBodyTailRight, 5),
        ],
        [P.BodySlow] =
        [
            Show(EnemyFrameId.PowampBodyTailLeft, 9),
            Show(EnemyFrameId.PowampBodyTailDown, 9),
            Show(EnemyFrameId.PowampBodyTailRight, 9),
        ],
        [P.BalloonInflatingFromDeflated] =
        [
            Show(EnemyFrameId.PowampBalloonSmall, 1),
        ],
        [P.BalloonInflatingFromHalfway] =
        [
            Show(EnemyFrameId.PowampBalloonHalfInflated, 6),
            Show(EnemyFrameId.PowampBalloonInflated, 160),
        ],
        [P.BalloonStartSinking] =
        [
            Show(EnemyFrameId.PowampBalloonInflated, 1),
            Show(EnemyFrameId.PowampBalloonHalfInflated, 6),
        ],
        [P.BalloonDeflated] =
        [
            Show(EnemyFrameId.PowampBalloonSmall, 160),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<PowampSegment, Animation<PowampSegment>> StartingAt { get; } = new Dictionary<PowampSegment, Animation<PowampSegment>>
    {
        [P.BodyFast] = new([Segments.Whole(P.BodyFast)], AnimationEnd.LoopsFrom(0)),
        [P.BodySlow] = new([Segments.Whole(P.BodySlow)], AnimationEnd.LoopsFrom(0)),
        [P.BalloonInflatingFromDeflated] = new([Segments.Whole(P.BalloonInflatingFromDeflated), Segments.Whole(P.BalloonInflatingFromHalfway)], AnimationEnd.Holds),
        [P.BalloonInflatingFromHalfway] = new([Segments.Whole(P.BalloonInflatingFromHalfway)], AnimationEnd.Holds),
        [P.BalloonStartSinking] = new([Segments.Whole(P.BalloonStartSinking), Segments.Whole(P.BalloonDeflated)], AnimationEnd.Holds),
        [P.BalloonDeflated] = new([Segments.Whole(P.BalloonDeflated)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PowampSegment> StartingWith(PowampSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
