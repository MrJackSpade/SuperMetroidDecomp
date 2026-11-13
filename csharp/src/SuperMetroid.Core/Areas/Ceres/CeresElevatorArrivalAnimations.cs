using P = SuperMetroid.Core.Areas.Ceres.CeresElevatorArrivalSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres elevator arrival projectiles' typed animations (#641, #1600): the pad alternates two poses and the platform holds
/// one, each for one update at a time, until landing redirects both to the delete.
/// </summary>
internal static class CeresElevatorArrivalAnimations
{
    /// <summary>Every arrival segment's beats.</summary>
    internal static AnimationSegments<CeresElevatorArrivalSegment, CeresElevatorArrivalPose, NoAnimationCue> Segments { get; } = new(
        new Dictionary<CeresElevatorArrivalSegment, AnimationBeat<CeresElevatorArrivalPose, NoAnimationCue>[]>
    {
        [P.Pad] =
        [
            Show(CeresElevatorArrivalPose.PadFirst, 1),
            Show(CeresElevatorArrivalPose.PadSecond, 1),
        ],
        [P.Platform] =
        [
            Show(CeresElevatorArrivalPose.Platform, 1),
        ],
        [P.Delete] = [],
    });

    /// <summary>The animation that starts at each segment: the pad and platform loop, and the delete ends at once.</summary>
    internal static IReadOnlyDictionary<CeresElevatorArrivalSegment, Animation<CeresElevatorArrivalSegment>> StartingAt { get; } = new Dictionary<CeresElevatorArrivalSegment, Animation<CeresElevatorArrivalSegment>>
    {
        [P.Pad] = new([new(P.Pad, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Platform] = new([new(P.Platform, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Delete] = new([], AnimationEnd.Stops),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CeresElevatorArrivalSegment> StartingWith(CeresElevatorArrivalSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<CeresElevatorArrivalPose, NoAnimationCue> Show(CeresElevatorArrivalPose pose, int duration) =>
        new AnimationBeat<CeresElevatorArrivalPose, NoAnimationCue>.Frame(pose, duration);

    /// <summary>The composition each arrival pose shows (<see cref="CeresElevatorArrivalDefinitions.Frames"/>).</summary>
    internal static EnemyProjectileFrameId Frame(CeresElevatorArrivalPose pose) => Ensure.Listed(CeresElevatorArrivalDefinitions.Frames, pose);
}
