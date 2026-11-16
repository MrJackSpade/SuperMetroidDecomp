using P = SuperMetroid.Core.Enemies.Ripper.RipperSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>
/// The grapple Ripper, jet Ripper and Ripper flying loops (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class RipperAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<RipperSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<RipperSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.GrappleRipperMovingLeft] =
        [
            Show(EnemyFrameId.GrappleRipperMovingLeftFirst, 8),
            Show(EnemyFrameId.GrappleRipperMovingLeftSecond, 7),
            Show(EnemyFrameId.GrappleRipperMovingLeftFirst, 8),
            Show(EnemyFrameId.GrappleRipperMovingLeftThird, 7),
        ],
        [P.GrappleRipperMovingRight] =
        [
            Show(EnemyFrameId.GrappleRipperMovingRightFirst, 8),
            Show(EnemyFrameId.GrappleRipperMovingRightSecond, 7),
            Show(EnemyFrameId.GrappleRipperMovingRightFirst, 8),
            Show(EnemyFrameId.GrappleRipperMovingRightThird, 7),
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
        [P.GrappleRipperMovingLeft] = new([new(P.GrappleRipperMovingLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GrappleRipperMovingRight] = new([new(P.GrappleRipperMovingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RipperMovingRight] = new([new(P.RipperMovingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RipperMovingLeft] = new([new(P.RipperMovingLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<RipperSegment> StartingWith(RipperSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
