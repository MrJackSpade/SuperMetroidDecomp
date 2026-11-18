using P = SuperMetroid.Core.Areas.Ceres.CeresElevatorArrivalSegment;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres elevator arrival actors' typed animations (#641, #1600): the pad alternates two poses, each for one update
/// at a time, and the platform rests on its one pose, until both leave as Samus lands (#2424).
/// </summary>
internal static class CeresElevatorArrivalAnimations
{
    /// <summary>Every arrival segment's beats.</summary>
    internal static AnimationSegments<CeresElevatorArrivalSegment, CeresElevatorArrivalPose, NoAnimationCue> Segments { get; } = new(
        new Dictionary<CeresElevatorArrivalSegment, AnimationBeat<CeresElevatorArrivalPose, NoAnimationCue>[]>
    {
        [P.Pad] =
        [
            Show(CeresElevatorArrivalPose.PadUnlit, 1),
            Show(CeresElevatorArrivalPose.PadLit, 1),
        ],
        [P.Platform] =
        [
            new AnimationBeat<CeresElevatorArrivalPose, NoAnimationCue>.HeldFrame(CeresElevatorArrivalPose.Platform),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment: the pad loops its two poses and the platform rests on its pose for good.
    /// </summary>
    internal static IReadOnlyDictionary<CeresElevatorArrivalSegment, Animation<CeresElevatorArrivalSegment>> StartingAt { get; } = new Dictionary<CeresElevatorArrivalSegment, Animation<CeresElevatorArrivalSegment>>
    {
        [P.Pad] = Animation<CeresElevatorArrivalSegment>.Looping([Segments.Whole(P.Pad)]),
        [P.Platform] = new([Segments.Whole(P.Platform)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CeresElevatorArrivalSegment> StartingWith(CeresElevatorArrivalSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<CeresElevatorArrivalPose, NoAnimationCue> Show(CeresElevatorArrivalPose pose, int duration) =>
        new AnimationBeat<CeresElevatorArrivalPose, NoAnimationCue>.Frame(pose, duration);
}
