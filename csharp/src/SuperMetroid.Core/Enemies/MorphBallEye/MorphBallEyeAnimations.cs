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
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MorphBallEyeSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<MorphBallEyeSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.FacingRightDeactivating] =
        [
            Show(EnemyFrameId.MorphEyeRightTransition, 8),
            Show(EnemyFrameId.MorphBallEyeLookingLeft, 48),
            Show(EnemyFrameId.MorphEyeRightTransition, 5),
        ],
        [P.FacingRightClosed] =
        [
            Show(EnemyFrameId.MorphEyeRightClosed, 48),
        ],
        [P.FacingLeftDeactivating] =
        [
            Show(EnemyFrameId.MorphEyeLeftTransition, 8),
            Show(EnemyFrameId.MorphBallEyeLookingRight, 48),
            Show(EnemyFrameId.MorphEyeLeftTransition, 5),
        ],
        [P.FacingLeftClosed] =
        [
            Show(EnemyFrameId.MorphEyeLeftClosed, 48),
        ],
        [P.FacingRightActivating] =
        [
            Show(EnemyFrameId.MorphEyeRightClosed, 32),
            Show(EnemyFrameId.MorphEyeRightTransition, 5),
            Show(EnemyFrameId.MorphBallEyeLookingLeft, 48),
            Show(EnemyFrameId.MorphEyeRightTransition, 8),
        ],
        [P.FacingLeftActivating] =
        [
            Show(EnemyFrameId.MorphEyeLeftClosed, 32),
            Show(EnemyFrameId.MorphEyeLeftTransition, 5),
            Show(EnemyFrameId.MorphBallEyeLookingRight, 48),
            Show(EnemyFrameId.MorphEyeLeftTransition, 8),
        ],
        [P.MountFacingRight] =
        [
            Show(EnemyFrameId.MorphEyeMountRight, 1),
        ],
        [P.MountFacingDown] =
        [
            Show(EnemyFrameId.MorphEyeMountDown, 1),
        ],
        [P.MountFacingLeft] =
        [
            Show(EnemyFrameId.MorphEyeMountLeft, 1),
        ],
        [P.MountFacingUp] =
        [
            Show(EnemyFrameId.MorphEyeMountUp, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<MorphBallEyeSegment, Animation<MorphBallEyeSegment>> StartingAt { get; } = new Dictionary<MorphBallEyeSegment, Animation<MorphBallEyeSegment>>
    {
        [P.FacingRightDeactivating] = new([new(P.FacingRightDeactivating, 0, 3, 1), new(P.FacingRightClosed, 0, 1, 1)], AnimationEnd.Holds),
        [P.FacingRightClosed] = new([new(P.FacingRightClosed, 0, 1, 1)], AnimationEnd.Holds),
        [P.FacingLeftDeactivating] = new([new(P.FacingLeftDeactivating, 0, 3, 1), new(P.FacingLeftClosed, 0, 1, 1)], AnimationEnd.Holds),
        [P.FacingLeftClosed] = new([new(P.FacingLeftClosed, 0, 1, 1)], AnimationEnd.Holds),
        [P.FacingRightActivating] = new([new(P.FacingRightActivating, 0, 4, 1)], AnimationEnd.Holds),
        [P.FacingLeftActivating] = new([new(P.FacingLeftActivating, 0, 4, 1)], AnimationEnd.Holds),
        [P.MountFacingRight] = new([new(P.MountFacingRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.MountFacingDown] = new([new(P.MountFacingDown, 0, 1, 1)], AnimationEnd.Holds),
        [P.MountFacingLeft] = new([new(P.MountFacingLeft, 0, 1, 1)], AnimationEnd.Holds),
        [P.MountFacingUp] = new([new(P.MountFacingUp, 0, 1, 1)], AnimationEnd.Holds),
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
}
