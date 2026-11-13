using P = SuperMetroid.Core.Enemies.Ripper.RipperSegment;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>
/// The GRipper, Ripper II and Ripper flying loops as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and its lists use no owner callbacks.
/// </summary>
internal static class RipperAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<RipperSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<RipperSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.GRipperMovingLeft] =
        [
            Show(EnemyFrameId.GRipperMovingLeftFirst, 8),
            Show(EnemyFrameId.GRipperMovingLeftSecond, 7),
            Show(EnemyFrameId.GRipperMovingLeftFirst, 8),
            Show(EnemyFrameId.GRipperMovingLeftThird, 7),
        ],
        [P.GRipperMovingRight] =
        [
            Show(EnemyFrameId.GRipperMovingRightFirst, 8),
            Show(EnemyFrameId.GRipperMovingRightSecond, 7),
            Show(EnemyFrameId.GRipperMovingRightFirst, 8),
            Show(EnemyFrameId.GRipperMovingRightThird, 7),
        ],
        [P.Ripper2MovingRight] =
        [
            Show(EnemyFrameId.GRipperMovingLeftFirst, 8),
            Show(EnemyFrameId.GRipperMovingLeftSecond, 7),
            Show(EnemyFrameId.GRipperMovingLeftFirst, 8),
            Show(EnemyFrameId.GRipperMovingLeftThird, 7),
        ],
        [P.Ripper2MovingLeft] =
        [
            Show(EnemyFrameId.GRipperMovingRightFirst, 8),
            Show(EnemyFrameId.GRipperMovingRightSecond, 7),
            Show(EnemyFrameId.GRipperMovingRightFirst, 8),
            Show(EnemyFrameId.GRipperMovingRightThird, 7),
        ],
        [P.RipperMovingRight] =
        [
            Show(EnemyFrameId.RipperMovingRightFirst, 8),
            Show(EnemyFrameId.RipperMovingRightSecond, 7),
            Show(EnemyFrameId.RipperMovingRightFirst, 8),
            Show(EnemyFrameId.RipperMovingRightThird, 7),
        ],
        [P.RipperMovingLeft] =
        [
            Show(EnemyFrameId.RipperMovingLeftFirst, 8),
            Show(EnemyFrameId.RipperMovingLeftSecond, 7),
            Show(EnemyFrameId.RipperMovingLeftFirst, 8),
            Show(EnemyFrameId.RipperMovingLeftThird, 7),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<RipperSegment, Animation<RipperSegment>> StartingAt { get; } = new Dictionary<RipperSegment, Animation<RipperSegment>>
    {
        [P.GRipperMovingLeft] = new([new(P.GRipperMovingLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GRipperMovingRight] = new([new(P.GRipperMovingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Ripper2MovingRight] = new([new(P.Ripper2MovingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Ripper2MovingLeft] = new([new(P.Ripper2MovingLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RipperMovingRight] = new([new(P.RipperMovingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RipperMovingLeft] = new([new(P.RipperMovingLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<RipperSegment> StartingWith(RipperSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
