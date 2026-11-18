using P = SuperMetroid.Core.Enemies.MorphBallEye.MorphBallEyeSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// The Morph Ball eye's eyelid and mount animations (#641), and the pose the open eye looks along as it tracks Samus
/// (#2070). Each frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class MorphBallEyeAnimations
{
    /// <summary>
    /// Every segment's beats. An opening ends on its half-open lid (#2376): the eye opens on the update the lid first shows,
    /// and its tracking takes over the picture on the next.
    /// </summary>
    internal static AnimationSegments<MorphBallEyeSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<MorphBallEyeSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.FacingRightDeactivating] =
        [
            Show(EnemyFrameId.MorphBallEyeFacingRightHalfOpen, 8),
            Show(EnemyFrameId.MorphBallEyeLookingLeft, 48),
            Show(EnemyFrameId.MorphBallEyeFacingRightHalfOpen, 5),
        ],
        [P.FacingRightClosed] =
        [
            Hold(EnemyFrameId.MorphBallEyeFacingRightClosed),
        ],
        [P.FacingLeftDeactivating] =
        [
            Show(EnemyFrameId.MorphBallEyeFacingLeftHalfOpen, 8),
            Show(EnemyFrameId.MorphBallEyeLookingRight, 48),
            Show(EnemyFrameId.MorphBallEyeFacingLeftHalfOpen, 5),
        ],
        [P.FacingLeftClosed] =
        [
            Hold(EnemyFrameId.MorphBallEyeFacingLeftClosed),
        ],
        [P.FacingRightActivating] =
        [
            Show(EnemyFrameId.MorphBallEyeFacingRightClosed, 32),
            Show(EnemyFrameId.MorphBallEyeFacingRightHalfOpen, 5),
        ],
        [P.FacingLeftActivating] =
        [
            Show(EnemyFrameId.MorphBallEyeFacingLeftClosed, 32),
            Show(EnemyFrameId.MorphBallEyeFacingLeftHalfOpen, 5),
        ],
        [P.MountFacingRight] =
        [
            Hold(EnemyFrameId.MorphEyeMountRight),
        ],
        [P.MountFacingDown] =
        [
            Hold(EnemyFrameId.MorphEyeMountDown),
        ],
        [P.MountFacingLeft] =
        [
            Hold(EnemyFrameId.MorphEyeMountLeft),
        ],
        [P.MountFacingUp] =
        [
            Hold(EnemyFrameId.MorphEyeMountUp),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<MorphBallEyeSegment, Animation<MorphBallEyeSegment>> StartingAt { get; } = new Dictionary<MorphBallEyeSegment, Animation<MorphBallEyeSegment>>
    {
        [P.FacingRightDeactivating] = new([Segments.Whole(P.FacingRightDeactivating), Segments.Whole(P.FacingRightClosed)], AnimationEnd.Holds),
        [P.FacingRightClosed] = new([Segments.Whole(P.FacingRightClosed)], AnimationEnd.Holds),
        [P.FacingLeftDeactivating] = new([Segments.Whole(P.FacingLeftDeactivating), Segments.Whole(P.FacingLeftClosed)], AnimationEnd.Holds),
        [P.FacingLeftClosed] = new([Segments.Whole(P.FacingLeftClosed)], AnimationEnd.Holds),
        [P.FacingRightActivating] = new([Segments.Whole(P.FacingRightActivating)], AnimationEnd.Holds),
        [P.FacingLeftActivating] = new([Segments.Whole(P.FacingLeftActivating)], AnimationEnd.Holds),
        [P.MountFacingRight] = new([Segments.Whole(P.MountFacingRight)], AnimationEnd.Holds),
        [P.MountFacingDown] = new([Segments.Whole(P.MountFacingDown)], AnimationEnd.Holds),
        [P.MountFacingLeft] = new([Segments.Whole(P.MountFacingLeft)], AnimationEnd.Holds),
        [P.MountFacingUp] = new([Segments.Whole(P.MountFacingUp)], AnimationEnd.Holds),
    };

    /// <summary>
    /// The pose the open eye looks along for each sixteenth of the turn its angle to Samus lies in (#2070), the arcs
    /// following one another clockwise from straight up, as the eye's angle counts: fourteen poses, the two nearest
    /// straight up and straight down each looking a little to one side of it, and the eye keeps looking a little to the
    /// right through the arc after up's and a little to the left through the arc before it.
    /// </summary>
    internal static TurnArcs<EnemyFrameId> AimFrames { get; } = new(TrigonometryAngle.Zero,
        EnemyFrameId.MorphBallEyeLookingUpUpRight, EnemyFrameId.MorphBallEyeLookingUpUpRight, EnemyFrameId.MorphBallEyeLookingUpRight,
        EnemyFrameId.MorphBallEyeLookingRightUpRight, EnemyFrameId.MorphBallEyeLookingRight, EnemyFrameId.MorphBallEyeLookingRightDownRight,
        EnemyFrameId.MorphBallEyeLookingDownRight, EnemyFrameId.MorphBallEyeLookingDownDownRight, EnemyFrameId.MorphBallEyeLookingDownDownLeft,
        EnemyFrameId.MorphBallEyeLookingDownLeft, EnemyFrameId.MorphBallEyeLookingLeftDownLeft, EnemyFrameId.MorphBallEyeLookingLeft,
        EnemyFrameId.MorphBallEyeLookingLeftUpLeft, EnemyFrameId.MorphBallEyeLookingUpLeft, EnemyFrameId.MorphBallEyeLookingUpUpLeft,
        EnemyFrameId.MorphBallEyeLookingUpUpLeft);

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<MorphBallEyeSegment> StartingWith(MorphBallEyeSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good (#2337): a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.HeldFrame(frame);
}
