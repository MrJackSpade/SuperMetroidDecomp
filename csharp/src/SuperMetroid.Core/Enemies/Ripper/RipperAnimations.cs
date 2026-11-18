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
            Show(EnemyFrameId.GrappleRipperFlyingLeftFlameMedium, 8),
            Show(EnemyFrameId.GrappleRipperFlyingLeftFlameShort, 7),
            Show(EnemyFrameId.GrappleRipperFlyingLeftFlameMedium, 8),
            Show(EnemyFrameId.GrappleRipperFlyingLeftFlameLong, 7),
        ],
        [P.GrappleRipperMovingRight] =
        [
            Show(EnemyFrameId.GrappleRipperFlyingRightFlameMedium, 8),
            Show(EnemyFrameId.GrappleRipperFlyingRightFlameShort, 7),
            Show(EnemyFrameId.GrappleRipperFlyingRightFlameMedium, 8),
            Show(EnemyFrameId.GrappleRipperFlyingRightFlameLong, 7),
        ],
        [P.RipperMovingRight] =
        [
            Show(EnemyFrameId.RipperFlyingRightEyeLit, 8),
            Show(EnemyFrameId.RipperFlyingRightEyeDark, 7),
            Show(EnemyFrameId.RipperFlyingRightEyeLit, 8),
            Show(EnemyFrameId.RipperFlyingRightEyeGlinting, 7),
        ],
        [P.RipperMovingLeft] =
        [
            Show(EnemyFrameId.RipperFlyingLeftEyeLit, 8),
            Show(EnemyFrameId.RipperFlyingLeftEyeDark, 7),
            Show(EnemyFrameId.RipperFlyingLeftEyeLit, 8),
            Show(EnemyFrameId.RipperFlyingLeftEyeGlinting, 7),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<RipperSegment, Animation<RipperSegment>> StartingAt { get; } = new Dictionary<RipperSegment, Animation<RipperSegment>>
    {
        [P.GrappleRipperMovingLeft] = new([Segments.Whole(P.GrappleRipperMovingLeft)], AnimationEnd.LoopsFrom(0)),
        [P.GrappleRipperMovingRight] = new([Segments.Whole(P.GrappleRipperMovingRight)], AnimationEnd.LoopsFrom(0)),
        [P.RipperMovingRight] = new([Segments.Whole(P.RipperMovingRight)], AnimationEnd.LoopsFrom(0)),
        [P.RipperMovingLeft] = new([Segments.Whole(P.RipperMovingLeft)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<RipperSegment> StartingWith(RipperSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
