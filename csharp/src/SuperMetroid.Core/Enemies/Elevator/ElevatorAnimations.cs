using P = SuperMetroid.Core.Enemies.Elevator.ElevatorProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>
/// The elevator's two-frame animation loop as a typed program (#641), in place of their bank-$A3 lists. Generated from the verified native lists: each
/// frame shows a <see cref="ElevatorFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class ElevatorAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<ElevatorProgram, ElevatorFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<ElevatorProgram, AnimationProgramStep<ElevatorProgram, ElevatorFrame, NoAnimationCue>[]>
    {
        [P.Loop] =
        [
            Show(0, 2),
            Show(1, 2),
            GoTo(P.Loop),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<ElevatorProgram, ElevatorFrame, NoAnimationCue> Show(int pose, int duration) => AnimationProgramStep<ElevatorProgram, ElevatorFrame, NoAnimationCue>.Show(new(pose), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<ElevatorProgram, ElevatorFrame, NoAnimationCue> GoTo(ElevatorProgram program) => AnimationProgramStep<ElevatorProgram, ElevatorFrame, NoAnimationCue>.GoTo(program);

}
