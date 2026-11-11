using P = SuperMetroid.Core.Enemies.Elevator.ElevatorProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>
/// The elevator's two-frame animation loop as a typed program (#641). Each
/// frame shows a <see cref="ElevatorFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class ElevatorAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<ElevatorProgram, ElevatorFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<ElevatorProgram, AnimationBeat<ElevatorFrame, NoAnimationCue>[]>
    {
        [P.Loop] =
        [
            Show(0, 2),
            Show(1, 2),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<ElevatorProgram, Animation<ElevatorProgram>> StartingAt { get; } = new Dictionary<ElevatorProgram, Animation<ElevatorProgram>>
    {
        [P.Loop] = new([new(P.Loop, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<ElevatorProgram> StartingWith(ElevatorProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<ElevatorFrame, NoAnimationCue> Show(int pose, int duration) => new AnimationBeat<ElevatorFrame, NoAnimationCue>.Frame(new(pose), duration);
}
