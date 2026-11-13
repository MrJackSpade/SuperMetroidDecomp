using P = SuperMetroid.Core.Enemies.Powamp.PowampSegment;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// Powamp's body pulses and balloon inflate and sink animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and its lists use no owner callbacks.
/// </summary>
internal static class PowampAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<PowampSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<PowampSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.BodyFast] =
        [
            Show(EnemyFrameId.PowampBodyFastFirst, 5),
            Show(EnemyFrameId.PowampBodyFastSecond, 5),
            Show(EnemyFrameId.PowampBodyFastThird, 5),
        ],
        [P.BodySlow] =
        [
            Show(EnemyFrameId.PowampBodyFastFirst, 9),
            Show(EnemyFrameId.PowampBodyFastSecond, 9),
            Show(EnemyFrameId.PowampBodyFastThird, 9),
        ],
        [P.BalloonInflate0] =
        [
            Show(EnemyFrameId.PowampBalloonInflateFirst, 1),
        ],
        [P.BalloonInflate1] =
        [
            Show(EnemyFrameId.PowampBalloonInflateSecond, 6),
            Show(EnemyFrameId.PowampBalloonInflateThird, 160),
        ],
        [P.BalloonStartSinking] =
        [
            Show(EnemyFrameId.PowampBalloonInflateThird, 1),
            Show(EnemyFrameId.PowampBalloonInflateSecond, 6),
        ],
        [P.BalloonDeflated] =
        [
            Show(EnemyFrameId.PowampBalloonInflateFirst, 160),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<PowampSegment, Animation<PowampSegment>> StartingAt { get; } = new Dictionary<PowampSegment, Animation<PowampSegment>>
    {
        [P.BodyFast] = new([new(P.BodyFast, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.BodySlow] = new([new(P.BodySlow, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.BalloonInflate0] = new([new(P.BalloonInflate0, 0, 1, 1), new(P.BalloonInflate1, 0, 2, 1)], AnimationEnd.Holds),
        [P.BalloonInflate1] = new([new(P.BalloonInflate1, 0, 2, 1)], AnimationEnd.Holds),
        [P.BalloonStartSinking] = new([new(P.BalloonStartSinking, 0, 2, 1), new(P.BalloonDeflated, 0, 1, 1)], AnimationEnd.Holds),
        [P.BalloonDeflated] = new([new(P.BalloonDeflated, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PowampSegment> StartingWith(PowampSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
