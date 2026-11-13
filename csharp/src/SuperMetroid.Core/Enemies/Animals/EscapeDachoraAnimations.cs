using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Animals.EscapeDachoraCueKind;
using P = SuperMetroid.Core.Enemies.Animals.EscapeDachoraSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The escaping Dachora's typed animations (#641, #1600). Each frame shows a <see cref="EnemyFrameId"/> and
/// each cue is a <see cref="EscapeDachoraCue"/> with the values it acts on. Around the shaft it runs five laps
/// left and five right, counted on its timer; a branch continues the play where <see cref="BranchTargets"/> says.
/// </summary>
internal static class EscapeDachoraAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<EscapeDachoraSegment, EnemyFrameId, EscapeDachoraCue> Segments { get; } = new(
        new Dictionary<EscapeDachoraSegment, AnimationBeat<EnemyFrameId, EscapeDachoraCue>[]>
    {
        [P.RunningAroundLowTide] =
        [
        ],
        [P.RunningAroundLowTideLeft] =
        [
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideLeftFirst, 3),
            Cue(new(K.MoveLeft)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideLeftSecond, 3),
            Cue(new(K.MoveLeft)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideLeftThird, 3),
            Cue(new(K.MoveLeft)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideLeftFourth, 3),
            Cue(new(K.MoveLeft)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideLeftFifth, 3),
            Cue(new(K.MoveLeft)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideLeftSixth, 3),
            Cue(new(K.MoveLeft)),
            Cue(new(K.GoToIfAcidHigh, Target: P.RunningAroundHighTideLeftLoop)),
            Cue(new(K.GoToIfCrittersEscaped, Target: P.RunningForEscape)),
        ],
        [P.RunningAroundLowTideRight] =
        [
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightFirst, 3),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightSecond, 3),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightThird, 3),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightFourth, 3),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightFifth, 3),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightSixth, 3),
            Cue(new(K.MoveRight)),
            Cue(new(K.GoToIfAcidHigh, Target: P.RunningAroundHighTideRightLoop)),
            Cue(new(K.GoToIfCrittersEscaped, Target: P.RunningForEscapeAccelerating)),
        ],
        [P.RunningAroundHighTide] =
        [
        ],
        [P.RunningAroundHighTideLeft] =
        [
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideLeftFirst, 2),
            Cue(new(K.MoveLeft)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideLeftSecond, 2),
            Cue(new(K.MoveLeft)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideLeftThird, 2),
            Cue(new(K.MoveLeft)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideLeftFourth, 2),
            Cue(new(K.MoveLeft)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideLeftFifth, 2),
            Cue(new(K.MoveLeft)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideLeftSixth, 2),
            Cue(new(K.MoveLeft)),
            Cue(new(K.GoToIfCrittersEscaped, Target: P.RunningForEscape)),
        ],
        [P.RunningAroundHighTideLeftLoop] =
        [
        ],
        [P.RunningAroundHighTideRight] =
        [
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightFirst, 2),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightSecond, 2),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightThird, 2),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightFourth, 2),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightFifth, 2),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightSixth, 2),
            Cue(new(K.MoveRight)),
            Cue(new(K.GoToIfCrittersEscaped, Target: P.RunningForEscapeAccelerating)),
        ],
        [P.RunningAroundHighTideRightLoop] =
        [
        ],
        [P.RunningForEscape] =
        [
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideLeftFirst, 30),
        ],
        [P.RunningForEscapeAccelerating] =
        [
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightFirst, 90),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightSecond, 5),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightThird, 5),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightFourth, 4),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightFifth, 4),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightSixth, 4),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightFirst, 3),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightSecond, 3),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightThird, 3),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightFourth, 2),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightFifth, 2),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightSixth, 2),
            Cue(new(K.MoveRight)),
        ],
        [P.RunningForEscapeMaximumSpeed] =
        [
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightFirst, 1),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightSecond, 1),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightThird, 1),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightFourth, 1),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightFifth, 1),
            Cue(new(K.MoveRight)),
            Show(EnemyFrameId.EscapeDachoraRunningAroundLowTideRightSixth, 1),
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
    internal static IReadOnlyDictionary<EscapeDachoraSegment, Animation<EscapeDachoraSegment>> StartingAt { get; } = new Dictionary<EscapeDachoraSegment, Animation<EscapeDachoraSegment>>
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
    internal static IReadOnlyDictionary<EscapeDachoraSegment, AnimationContinuation<EscapeDachoraSegment>> BranchTargets { get; } =
        new Dictionary<EscapeDachoraSegment, AnimationContinuation<EscapeDachoraSegment>>
        {
            [P.RunningAroundHighTideLeftLoop] = new(RunningAroundHighTide, new AnimationPlace(0, 13)),
            [P.RunningAroundHighTideRightLoop] = new(RunningAroundHighTide, new AnimationPlace(1, 13)),
            [P.RunningForEscape] = new(RunningForEscape, AnimationPlace.StartOfAnimation),
            [P.RunningForEscapeAccelerating] = new(RunningForEscapeAccelerating, AnimationPlace.StartOfAnimation),
        };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<EscapeDachoraSegment> StartingWith(EscapeDachoraSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, EscapeDachoraCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, EscapeDachoraCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, EscapeDachoraCue> Cue(EscapeDachoraCue cue) => new AnimationBeat<EnemyFrameId, EscapeDachoraCue>.Event(cue);
}
