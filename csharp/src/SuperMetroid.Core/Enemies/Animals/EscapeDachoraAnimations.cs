using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Animals.EscapeDachoraCueKind;
using P = SuperMetroid.Core.Enemies.Animals.EscapeDachoraProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The escaping Dachora's typed animations (#641, #1600). Each frame shows a <see cref="EnemyFrameId"/> and
/// each callback becomes a <see cref="EscapeDachoraCue"/> with its typed operands. Around the shaft it runs five laps
/// left and five right, counted on its timer; a branch continues the play where <see cref="BranchTargets"/> says.
/// </summary>
internal static class EscapeDachoraAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<EscapeDachoraProgram, EnemyFrameId, EscapeDachoraCue> Segments { get; } = new(
        new Dictionary<EscapeDachoraProgram, AnimationBeat<EnemyFrameId, EscapeDachoraCue>[]>
    {
        [P.RunningAroundLowTide] =
        [
        ],
        [P.RunningAroundLowTideLeft] =
        [
            Show(EnemyFrameId.EscapeDachora00, 3),
            Cue(new(K.MoveLeft)),
            Show(EnemyFrameId.EscapeDachora01, 3),
            Cue(new(K.MoveLeft)),
            Show(EnemyFrameId.EscapeDachora02, 3),
            Cue(new(K.MoveLeft)),
            Show(EnemyFrameId.EscapeDachora03, 3),
            Cue(new(K.MoveLeft)),
            Show(EnemyFrameId.EscapeDachora04, 3),
            Cue(new(K.MoveLeft)),
            Show(EnemyFrameId.EscapeDachora05, 3),
            Cue(new(K.MoveLeft)),
            Cue(new(K.GoToIfAcidHigh, Target: P.RunningAroundHighTideLeftLoop)),
            Cue(new(K.GoToIfCrittersEscaped, Target: P.RunningForEscape)),
        ],
        [P.RunningAroundLowTideRight] =
        [
            Show(EnemyFrameId.EscapeDachora06, 3),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachora07, 3),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachora08, 3),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachora09, 3),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachora10, 3),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachora11, 3),
            Cue(new(K.MoveRight)),
            Cue(new(K.GoToIfAcidHigh, Target: P.RunningAroundHighTideRightLoop)),
            Cue(new(K.GoToIfCrittersEscaped, Target: P.RunningForEscapeAccelerating)),
        ],
        [P.RunningAroundHighTide] =
        [
        ],
        [P.RunningAroundHighTideLeft] =
        [
            Show(EnemyFrameId.EscapeDachora00, 2),
            Cue(new(K.MoveLeft)),
            Show(EnemyFrameId.EscapeDachora01, 2),
            Cue(new(K.MoveLeft)),
            Show(EnemyFrameId.EscapeDachora02, 2),
            Cue(new(K.MoveLeft)),
            Show(EnemyFrameId.EscapeDachora03, 2),
            Cue(new(K.MoveLeft)),
            Show(EnemyFrameId.EscapeDachora04, 2),
            Cue(new(K.MoveLeft)),
            Show(EnemyFrameId.EscapeDachora05, 2),
            Cue(new(K.MoveLeft)),
            Cue(new(K.GoToIfCrittersEscaped, Target: P.RunningForEscape)),
        ],
        [P.RunningAroundHighTideLeftLoop] =
        [
        ],
        [P.RunningAroundHighTideRight] =
        [
            Show(EnemyFrameId.EscapeDachora06, 2),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachora07, 2),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachora08, 2),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachora09, 2),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachora10, 2),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachora11, 2),
            Cue(new(K.MoveRight)),
            Cue(new(K.GoToIfCrittersEscaped, Target: P.RunningForEscapeAccelerating)),
        ],
        [P.RunningAroundHighTideRightLoop] =
        [
        ],
        [P.RunningForEscape] =
        [
            Show(EnemyFrameId.EscapeDachora00, 30),
        ],
        [P.RunningForEscapeAccelerating] =
        [
            Show(EnemyFrameId.EscapeDachora06, 90),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachora07, 5),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachora08, 5),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachora09, 4),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachora10, 4),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachora11, 4),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachora06, 3),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachora07, 3),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachora08, 3),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachora09, 2),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachora10, 2),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachora11, 2),
            Cue(new(K.MoveRight)),
        ],
        [P.RunningForEscapeMaximumSpeed] =
        [
            Show(EnemyFrameId.EscapeDachora06, 1),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachora07, 1),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachora08, 1),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachora09, 1),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachora10, 1),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachora11, 1),
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

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, EscapeDachoraCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, EscapeDachoraCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, EscapeDachoraCue> Cue(EscapeDachoraCue cue) => new AnimationBeat<EnemyFrameId, EscapeDachoraCue>.Event(cue);
}
