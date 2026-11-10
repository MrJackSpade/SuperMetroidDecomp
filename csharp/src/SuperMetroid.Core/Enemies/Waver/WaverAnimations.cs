using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Waver.WaverCueKind;
using P = SuperMetroid.Core.Enemies.Waver.WaverProgram;

namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>
/// Waver's steady and spinning animations as typed programs (#641). Each
/// frame shows a <see cref="WaverFrame"/> pose and each callback becomes a <see cref="WaverCue"/> with its typed operands.
/// </summary>
internal static class WaverAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<WaverProgram, WaverFrame, WaverCue> Segments { get; } = new(
        new Dictionary<WaverProgram, AnimationBeat<WaverFrame, WaverCue>[]>
    {
        [P.SteadyFacingLeft] =
        [
            Show(2, 1),
        ],
        [P.SteadyFacingRight] =
        [
            Show(7, 1),
        ],
        [P.SpinningFacingLeft] =
        [
            Show(3, 8),
            Show(4, 8),
            Show(0, 8),
            Show(1, 8),
            Cue(new(K.FinishSpin)),
        ],
        [P.SpinningFacingRight] =
        [
            Show(8, 8),
            Show(9, 8),
            Show(5, 8),
            Show(6, 8),
            Cue(new(K.FinishSpin)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<WaverProgram, Animation<WaverProgram>> StartingAt { get; } = new Dictionary<WaverProgram, Animation<WaverProgram>>
    {
        [P.SteadyFacingLeft] = new([new(P.SteadyFacingLeft, 0, 1, 1)], AnimationEnd.Holds),
        [P.SteadyFacingRight] = new([new(P.SteadyFacingRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.SpinningFacingLeft] = new([new(P.SpinningFacingLeft, 0, 5, 1)], AnimationEnd.Holds),
        [P.SpinningFacingRight] = new([new(P.SpinningFacingRight, 0, 5, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<WaverProgram> StartingWith(WaverProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<WaverFrame, WaverCue> Show(int pose, int duration) => new AnimationBeat<WaverFrame, WaverCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<WaverFrame, WaverCue> Cue(WaverCue cue) => new AnimationBeat<WaverFrame, WaverCue>.Event(cue);
}
