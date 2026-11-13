using P = SuperMetroid.Core.Enemies.MorphBallEye.MorphBallEyeSegment;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// The Morph Ball eye's tracking, eyelid and mount animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class MorphBallEyeAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MorphBallEyeSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<MorphBallEyeSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Active] =
        [
            Show(EnemyFrameId.MorphBallEyeActiveFirst, 10),
            Show(EnemyFrameId.MorphBallEyeActiveFirst, 10),
            Show(EnemyFrameId.MorphBallEyeActiveSecond, 10),
            Show(EnemyFrameId.MorphBallEyeActiveThird, 10),
            Show(EnemyFrameId.MorphBallEyeActiveFourth, 10),
            Show(EnemyFrameId.MorphBallEyeActiveFifth, 10),
            Show(EnemyFrameId.MorphBallEyeActiveSixth, 10),
            Show(EnemyFrameId.MorphBallEyeActiveSeventh, 10),
            Show(EnemyFrameId.MorphBallEyeActiveEighth, 10),
            Show(EnemyFrameId.MorphBallEyeActiveNinth, 10),
            Show(EnemyFrameId.MorphBallEyeActiveTenth, 10),
            Show(EnemyFrameId.MorphBallEyeActiveEleventh, 10),
            Show(EnemyFrameId.MorphBallEyeActiveTwelfth, 10),
            Show(EnemyFrameId.MorphBallEyeActiveThirteenth, 10),
            Show(EnemyFrameId.MorphBallEyeActiveFourteenth, 10),
            Show(EnemyFrameId.MorphBallEyeActiveFourteenth, 10),
        ],
        [P.FacingRightDeactivating] =
        [
            Show(EnemyFrameId.MorphEyeRightTransition, 8),
            Show(EnemyFrameId.MorphBallEyeActiveEleventh, 48),
            Show(EnemyFrameId.MorphEyeRightTransition, 5),
        ],
        [P.FacingRightClosed] =
        [
            Show(EnemyFrameId.MorphEyeRightClosed, 48),
        ],
        [P.FacingLeftDeactivating] =
        [
            Show(EnemyFrameId.MorphEyeLeftTransition, 8),
            Show(EnemyFrameId.MorphBallEyeActiveFourth, 48),
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
            Show(EnemyFrameId.MorphBallEyeActiveEleventh, 48),
            Show(EnemyFrameId.MorphEyeRightTransition, 8),
        ],
        [P.FacingLeftActivating] =
        [
            Show(EnemyFrameId.MorphEyeLeftClosed, 32),
            Show(EnemyFrameId.MorphEyeLeftTransition, 5),
            Show(EnemyFrameId.MorphBallEyeActiveFourth, 48),
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
        [P.Active] = new([new(P.Active, 0, 16, 1)], AnimationEnd.LoopsFrom(0)),
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
    /// The place in the tracking loop each part of the turn resumes at: the loop's poses follow the turn, one for each of
    /// its sixteen equal arcs from the eye's angle zero.
    /// </summary>
    internal static TurnArcs<AnimationPlace> TrackingPlaces { get; } = new(TrigonometryAngle.Zero,
        [.. Enumerable.Range(0, MorphBallEyeBeamDefinitions.TrackingPoses).Select(pose =>
            AnimationSegments<MorphBallEyeSegment, EnemyFrameId, NoAnimationCue>.PlaceOf(StartingAt[P.Active], P.Active, pose))]);

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<MorphBallEyeSegment> StartingWith(MorphBallEyeSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
