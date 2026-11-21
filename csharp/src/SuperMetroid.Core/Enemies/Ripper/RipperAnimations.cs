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
        [P.GrappleRipperMovingLeft] = Flying(EnemyFrameId.GrappleRipperFlyingLeftFlameMedium, EnemyFrameId.GrappleRipperFlyingLeftFlameShort, EnemyFrameId.GrappleRipperFlyingLeftFlameLong),
        [P.GrappleRipperMovingRight] = Flying(EnemyFrameId.GrappleRipperFlyingRightFlameMedium, EnemyFrameId.GrappleRipperFlyingRightFlameShort, EnemyFrameId.GrappleRipperFlyingRightFlameLong),
        [P.RipperMovingRight] = Flying(EnemyFrameId.RipperFlyingRightEyeLit, EnemyFrameId.RipperFlyingRightEyeDark, EnemyFrameId.RipperFlyingRightEyeGlinting),
        [P.RipperMovingLeft] = Flying(EnemyFrameId.RipperFlyingLeftEyeLit, EnemyFrameId.RipperFlyingLeftEyeDark, EnemyFrameId.RipperFlyingLeftEyeGlinting),
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static Dictionary<RipperSegment, Animation<RipperSegment>> StartingAt { get; } = new Dictionary<RipperSegment, Animation<RipperSegment>>
    {
        [P.GrappleRipperMovingLeft] = Animation<RipperSegment>.Looping([Segments.Whole(P.GrappleRipperMovingLeft)]),
        [P.GrappleRipperMovingRight] = Animation<RipperSegment>.Looping([Segments.Whole(P.GrappleRipperMovingRight)]),
        [P.RipperMovingRight] = Animation<RipperSegment>.Looping([Segments.Whole(P.RipperMovingRight)]),
        [P.RipperMovingLeft] = Animation<RipperSegment>.Looping([Segments.Whole(P.RipperMovingLeft)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<RipperSegment> StartingWith(RipperSegment segment) => StartingAt[segment];

    /// <summary>A Ripper's flying loop over its facing's looks, <paramref name="steady"/>, <paramref name="dimmer"/>, <paramref name="brighter"/>: its first look again between the others, eight and seven updates in turn.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue>[] Flying(EnemyFrameId steady, EnemyFrameId dimmer, EnemyFrameId brighter) =>
    [
        Show(steady, 8),
        Show(dimmer, 7),
        Show(steady, 8),
        Show(brighter, 7),
    ];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
