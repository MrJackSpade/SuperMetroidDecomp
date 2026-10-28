using P = SuperMetroid.Core.Areas.Ceres.CeresElevatorArrivalProgram;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres elevator arrival projectiles' typed programs (#641): the pad alternates two poses and the platform holds
/// one, each for one update at a time, until landing redirects both to the delete.
/// </summary>
internal static class CeresElevatorArrivalAnimations
{
    /// <summary>Every arrival program.</summary>
    internal static AnimationProgramSet<CeresElevatorArrivalProgram, CeresElevatorArrivalPose, NoAnimationCue> Programs { get; } = new(
        new Dictionary<CeresElevatorArrivalProgram, AnimationProgramStep<CeresElevatorArrivalProgram, CeresElevatorArrivalPose, NoAnimationCue>[]>
    {
        [P.Pad] =
        [
            Show(CeresElevatorArrivalPose.PadFirst, 1),
            Show(CeresElevatorArrivalPose.PadSecond, 1),
            AnimationProgramStep<CeresElevatorArrivalProgram, CeresElevatorArrivalPose, NoAnimationCue>.GoTo(P.Pad),
        ],
        [P.Platform] =
        [
            Show(CeresElevatorArrivalPose.Platform, 1),
            AnimationProgramStep<CeresElevatorArrivalProgram, CeresElevatorArrivalPose, NoAnimationCue>.GoTo(P.Platform),
        ],
        [P.Delete] = [AnimationProgramStep<CeresElevatorArrivalProgram, CeresElevatorArrivalPose, NoAnimationCue>.Stop],
    });

    /// <summary>Shows <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<CeresElevatorArrivalProgram, CeresElevatorArrivalPose, NoAnimationCue> Show(
        CeresElevatorArrivalPose pose, int duration) =>
        AnimationProgramStep<CeresElevatorArrivalProgram, CeresElevatorArrivalPose, NoAnimationCue>.Show(pose, duration);

    /// <summary>The composition each arrival pose shows (<see cref="CeresElevatorArrivalDefinitions.Frames"/>).</summary>
    internal static EnemyProjectileFrameId Frame(CeresElevatorArrivalPose pose) => Ensure.Listed(CeresElevatorArrivalDefinitions.Frames, pose);
}
