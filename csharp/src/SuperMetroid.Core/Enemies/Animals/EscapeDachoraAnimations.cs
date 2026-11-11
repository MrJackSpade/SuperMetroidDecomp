using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Animals.EscapeDachoraCueKind;
using P = SuperMetroid.Core.Enemies.Animals.EscapeDachoraProgram;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The escaping Dachora's typed animations (#641, #1600). Each frame shows a <see cref="EscapeDachoraFrame"/> pose and
/// each callback becomes a <see cref="EscapeDachoraCue"/> with its typed operands. Around the shaft it runs five laps
/// left and five right, counted on its timer; a branch continues the play where <see cref="BranchTargets"/> says.
/// </summary>
internal static class EscapeDachoraAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<EscapeDachoraProgram, EscapeDachoraFrame, EscapeDachoraCue> Segments { get; } = new(
        new Dictionary<EscapeDachoraProgram, AnimationBeat<EscapeDachoraFrame, EscapeDachoraCue>[]>
    {
        [P.RunningAroundLowTide] =
        [
        ],
        [P.RunningAroundLowTideLeft] =
        [
            Show(0, 3),
            Cue(new(K.MoveLeft)),
            Show(1, 3),
            Cue(new(K.MoveLeft)),
            Show(2, 3),
            Cue(new(K.MoveLeft)),
            Show(3, 3),
            Cue(new(K.MoveLeft)),
            Show(4, 3),
            Cue(new(K.MoveLeft)),
            Show(5, 3),
            Cue(new(K.MoveLeft)),
            Cue(new(K.GoToIfAcidHigh, Target: P.RunningAroundHighTideLeftLoop)),
            Cue(new(K.GoToIfCrittersEscaped, Target: P.RunningForEscape)),
        ],
        [P.RunningAroundLowTideRight] =
        [
            Show(6, 3),
            Cue(new(K.MoveRight)),
            Show(7, 3),
            Cue(new(K.MoveRight)),
            Show(8, 3),
            Cue(new(K.MoveRight)),
            Show(9, 3),
            Cue(new(K.MoveRight)),
            Show(10, 3),
            Cue(new(K.MoveRight)),
            Show(11, 3),
            Cue(new(K.MoveRight)),
            Cue(new(K.GoToIfAcidHigh, Target: P.RunningAroundHighTideRightLoop)),
            Cue(new(K.GoToIfCrittersEscaped, Target: P.RunningForEscapeAccelerating)),
        ],
        [P.RunningAroundHighTide] =
        [
        ],
        [P.RunningAroundHighTideLeft] =
        [
            Show(0, 2),
            Cue(new(K.MoveLeft)),
            Show(1, 2),
            Cue(new(K.MoveLeft)),
            Show(2, 2),
            Cue(new(K.MoveLeft)),
            Show(3, 2),
            Cue(new(K.MoveLeft)),
            Show(4, 2),
            Cue(new(K.MoveLeft)),
            Show(5, 2),
            Cue(new(K.MoveLeft)),
            Cue(new(K.GoToIfCrittersEscaped, Target: P.RunningForEscape)),
        ],
        [P.RunningAroundHighTideLeftLoop] =
        [
        ],
        [P.RunningAroundHighTideRight] =
        [
            Show(6, 2),
            Cue(new(K.MoveRight)),
            Show(7, 2),
            Cue(new(K.MoveRight)),
            Show(8, 2),
            Cue(new(K.MoveRight)),
            Show(9, 2),
            Cue(new(K.MoveRight)),
            Show(10, 2),
            Cue(new(K.MoveRight)),
            Show(11, 2),
            Cue(new(K.MoveRight)),
            Cue(new(K.GoToIfCrittersEscaped, Target: P.RunningForEscapeAccelerating)),
        ],
        [P.RunningAroundHighTideRightLoop] =
        [
        ],
        [P.RunningForEscape] =
        [
            Show(0, 30),
        ],
        [P.RunningForEscapeAccelerating] =
        [
            Show(6, 90),
            Cue(new(K.MoveRight)),
            Show(7, 5),
            Cue(new(K.MoveRight)),
            Show(8, 5),
            Cue(new(K.MoveRight)),
            Show(9, 4),
            Cue(new(K.MoveRight)),
            Show(10, 4),
            Cue(new(K.MoveRight)),
            Show(11, 4),
            Cue(new(K.MoveRight)),
            Show(6, 3),
            Cue(new(K.MoveRight)),
            Show(7, 3),
            Cue(new(K.MoveRight)),
            Show(8, 3),
            Cue(new(K.MoveRight)),
            Show(9, 2),
            Cue(new(K.MoveRight)),
            Show(10, 2),
            Cue(new(K.MoveRight)),
            Show(11, 2),
            Cue(new(K.MoveRight)),
        ],
        [P.RunningForEscapeMaximumSpeed] =
        [
            Show(6, 1),
            Cue(new(K.MoveRight)),
            Show(7, 1),
            Cue(new(K.MoveRight)),
            Show(8, 1),
            Cue(new(K.MoveRight)),
            Show(9, 1),
            Cue(new(K.MoveRight)),
            Show(10, 1),
            Cue(new(K.MoveRight)),
            Show(11, 1),
            Cue(new(K.MoveRight)),
        ],
    });

    /// <summary>Five laps left then five right at low tide, each lap counted on the timer, over and over.</summary>
    private static readonly Animation<P> RunningAroundLowTide =
        new([new(P.RunningAroundLowTideLeft, 0, 14, 5), new(P.RunningAroundLowTideRight, 0, 14, 5)], AnimationEnd.LoopsFrom(0));

    /// <summary>Five quicker laps left then five right at high tide, each lap counted on the timer, over and over.</summary>
    private static readonly Animation<P> RunningAroundHighTide =
        new([new(P.RunningAroundHighTideLeft, 0, 13, 5), new(P.RunningAroundHighTideRight, 0, 13, 5)], AnimationEnd.LoopsFrom(0));

    /// <summary>The pause, the acceleration, then the sprint at full speed for good.</summary>
    private static readonly Animation<P> RunningForEscape = new(
        [new(P.RunningForEscape, 0, 1, 1), new(P.RunningForEscapeAccelerating, 0, 24, 1), new(P.RunningForEscapeMaximumSpeed, 0, 12, 1)],
        AnimationEnd.LoopsFrom(2));

    /// <summary>The acceleration, then the sprint at full speed for good.</summary>
    private static readonly Animation<P> RunningForEscapeAccelerating = new(
        [new(P.RunningForEscapeAccelerating, 0, 24, 1), new(P.RunningForEscapeMaximumSpeed, 0, 12, 1)], AnimationEnd.LoopsFrom(1));

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<EscapeDachoraProgram, Animation<EscapeDachoraProgram>> StartingAt { get; } = new Dictionary<EscapeDachoraProgram, Animation<EscapeDachoraProgram>>
    {
        [P.RunningAroundLowTide] = RunningAroundLowTide,
        [P.RunningAroundHighTide] = RunningAroundHighTide,
        [P.RunningForEscape] = RunningForEscape,
        [P.RunningForEscapeAccelerating] = RunningForEscapeAccelerating,
        [P.RunningForEscapeMaximumSpeed] = new([new(P.RunningForEscapeMaximumSpeed, 0, 12, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>
    /// Where each branch target continues the play. A branch on high acid continues the high-tide laps at the end of a
    /// lap in the same direction, so the lap count already on the timer counts on; a branch for the escape starts the run.
    /// </summary>
    internal static IReadOnlyDictionary<EscapeDachoraProgram, AnimationContinuation<EscapeDachoraProgram>> BranchTargets { get; } =
        new Dictionary<EscapeDachoraProgram, AnimationContinuation<EscapeDachoraProgram>>
        {
            [P.RunningAroundHighTideLeftLoop] = new(RunningAroundHighTide, new AnimationPlace(0, 13)),
            [P.RunningAroundHighTideRightLoop] = new(RunningAroundHighTide, new AnimationPlace(1, 13)),
            [P.RunningForEscape] = new(RunningForEscape, AnimationPlace.StartOfAnimation),
            [P.RunningForEscapeAccelerating] = new(RunningForEscapeAccelerating, AnimationPlace.StartOfAnimation),
        };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<EscapeDachoraProgram> StartingWith(EscapeDachoraProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EscapeDachoraFrame, EscapeDachoraCue> Show(int pose, int duration) => new AnimationBeat<EscapeDachoraFrame, EscapeDachoraCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EscapeDachoraFrame, EscapeDachoraCue> Cue(EscapeDachoraCue cue) => new AnimationBeat<EscapeDachoraFrame, EscapeDachoraCue>.Event(cue);
}
