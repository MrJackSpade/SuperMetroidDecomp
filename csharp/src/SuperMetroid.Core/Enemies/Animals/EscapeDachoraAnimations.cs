using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Enemies.Animals.EscapeDachoraSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The escaping Dachora's typed animations (#641, #1600). Each frame shows a <see cref="EnemyFrameId"/> and
/// each cue is a <see cref="EscapeDachoraCue"/> with the values it acts on. Around the shaft it runs five laps
/// left and five right, counting its laps; each lap's end is a cue its owner may take elsewhere (#2061, #2167).
/// </summary>
internal static class EscapeDachoraAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<EscapeDachoraSegment, EnemyFrameId, EscapeDachoraCue> Segments { get; } = new(
        new Dictionary<EscapeDachoraSegment, AnimationBeat<EnemyFrameId, EscapeDachoraCue>[]>
    {
        [P.RunningAroundLowTideLeft] =
        [
            Show(EnemyFrameId.EscapeDachoraRunningLeftFarLegBack, 3),
            Cue(new EscapeDachoraCue.Move(Offset: -6)),
            Show(EnemyFrameId.EscapeDachoraRunningLeftFarLegPassing, 3),
            Cue(new EscapeDachoraCue.Move(Offset: -6)),
            Show(EnemyFrameId.EscapeDachoraRunningLeftFarLegReaching, 3),
            Cue(new EscapeDachoraCue.Move(Offset: -6)),
            Show(EnemyFrameId.EscapeDachoraRunningLeftNearLegBack, 3),
            Cue(new EscapeDachoraCue.Move(Offset: -6)),
            Show(EnemyFrameId.EscapeDachoraRunningLeftNearLegPassing, 3),
            Cue(new EscapeDachoraCue.Move(Offset: -6)),
            Show(EnemyFrameId.EscapeDachoraRunningLeftNearLegReaching, 3),
            Cue(new EscapeDachoraCue.Move(Offset: -6)),
            Cue(new EscapeDachoraCue.LowTideLapEnds(Lap: HorizontalDirection.Left)),
            Cue(new EscapeDachoraCue.LapEnds(Lap: HorizontalDirection.Left)),
        ],
        [P.RunningAroundLowTideRight] =
        [
            Show(EnemyFrameId.EscapeDachoraRunningRightFarLegBack, 3),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.EscapeDachoraRunningRightFarLegPassing, 3),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.EscapeDachoraRunningRightFarLegReaching, 3),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.EscapeDachoraRunningRightNearLegBack, 3),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.EscapeDachoraRunningRightNearLegPassing, 3),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.EscapeDachoraRunningRightNearLegReaching, 3),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Cue(new EscapeDachoraCue.LowTideLapEnds(Lap: HorizontalDirection.Right)),
            Cue(new EscapeDachoraCue.LapEnds(Lap: HorizontalDirection.Right)),
        ],
        [P.RunningAroundHighTideLeft] =
        [
            Show(EnemyFrameId.EscapeDachoraRunningLeftFarLegBack, 2),
            Cue(new EscapeDachoraCue.Move(Offset: -6)),
            Show(EnemyFrameId.EscapeDachoraRunningLeftFarLegPassing, 2),
            Cue(new EscapeDachoraCue.Move(Offset: -6)),
            Show(EnemyFrameId.EscapeDachoraRunningLeftFarLegReaching, 2),
            Cue(new EscapeDachoraCue.Move(Offset: -6)),
            Show(EnemyFrameId.EscapeDachoraRunningLeftNearLegBack, 2),
            Cue(new EscapeDachoraCue.Move(Offset: -6)),
            Show(EnemyFrameId.EscapeDachoraRunningLeftNearLegPassing, 2),
            Cue(new EscapeDachoraCue.Move(Offset: -6)),
            Show(EnemyFrameId.EscapeDachoraRunningLeftNearLegReaching, 2),
            Cue(new EscapeDachoraCue.Move(Offset: -6)),
            Cue(new EscapeDachoraCue.LapEnds(Lap: HorizontalDirection.Left)),
        ],
        [P.RunningAroundHighTideRight] =
        [
            Show(EnemyFrameId.EscapeDachoraRunningRightFarLegBack, 2),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.EscapeDachoraRunningRightFarLegPassing, 2),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.EscapeDachoraRunningRightFarLegReaching, 2),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.EscapeDachoraRunningRightNearLegBack, 2),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.EscapeDachoraRunningRightNearLegPassing, 2),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.EscapeDachoraRunningRightNearLegReaching, 2),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Cue(new EscapeDachoraCue.LapEnds(Lap: HorizontalDirection.Right)),
        ],
        [P.RunningForEscape] =
        [
            Show(EnemyFrameId.EscapeDachoraRunningLeftFarLegBack, 30),
        ],
        [P.RunningForEscapeAccelerating] =
        [
            Show(EnemyFrameId.EscapeDachoraRunningRightFarLegBack, 90),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.EscapeDachoraRunningRightFarLegPassing, 5),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.EscapeDachoraRunningRightFarLegReaching, 5),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.EscapeDachoraRunningRightNearLegBack, 4),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.EscapeDachoraRunningRightNearLegPassing, 4),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.EscapeDachoraRunningRightNearLegReaching, 4),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.EscapeDachoraRunningRightFarLegBack, 3),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.EscapeDachoraRunningRightFarLegPassing, 3),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.EscapeDachoraRunningRightFarLegReaching, 3),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.EscapeDachoraRunningRightNearLegBack, 2),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.EscapeDachoraRunningRightNearLegPassing, 2),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.EscapeDachoraRunningRightNearLegReaching, 2),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
        ],
        [P.RunningForEscapeMaximumSpeed] =
        [
            Show(EnemyFrameId.EscapeDachoraRunningRightFarLegBack, 1),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.EscapeDachoraRunningRightFarLegPassing, 1),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.EscapeDachoraRunningRightFarLegReaching, 1),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.EscapeDachoraRunningRightNearLegBack, 1),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.EscapeDachoraRunningRightNearLegPassing, 1),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.EscapeDachoraRunningRightNearLegReaching, 1),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
        ],
    });

    /// <summary>How many laps the escaping Dachora runs each way before it turns around.</summary>
    private const int LapsEachWay = 5;

    /// <summary>Five laps left then five right at low tide, each lap counted, over and over (#2061).</summary>
    internal static Animation<P> RunningAroundLowTide { get; } =
        new([Repeated(P.RunningAroundLowTideLeft, LapsEachWay), Repeated(P.RunningAroundLowTideRight, LapsEachWay)], AnimationEnd.LoopsFrom(0));

    /// <summary>Five quicker laps left then five right at high tide, each lap counted, over and over (#2061).</summary>
    internal static Animation<P> RunningAroundHighTide { get; } =
        new([Repeated(P.RunningAroundHighTideLeft, LapsEachWay), Repeated(P.RunningAroundHighTideRight, LapsEachWay)], AnimationEnd.LoopsFrom(0));

    /// <summary>The pause, the acceleration, then the sprint at full speed for good.</summary>
    internal static Animation<P> RunningForEscape { get; } = new(
        [Whole(P.RunningForEscape), Whole(P.RunningForEscapeAccelerating), Whole(P.RunningForEscapeMaximumSpeed)],
        AnimationEnd.LoopsFrom(2));

    /// <summary>The acceleration, then the sprint at full speed for good.</summary>
    internal static Animation<P> RunningForEscapeAccelerating { get; } = new(
        [Whole(P.RunningForEscapeAccelerating), Whole(P.RunningForEscapeMaximumSpeed)], AnimationEnd.LoopsFrom(1));

    /// <summary>
    /// Where a rising tide takes a lap to the left (#2061): the high-tide laps at the end of a lap to the left, counting on
    /// the laps the low-tide run had left.
    /// </summary>
    internal static AnimationContinuation<P> HighTideLapsAfterLeftLap { get; } =
        AnimationContinuation<P>.CountingOn(RunningAroundHighTide, RunningAroundHighTide.EndOfPlayOf(P.RunningAroundHighTideLeft));

    /// <summary>
    /// Where a rising tide takes a lap to the right (#2061): the high-tide laps at the end of a lap to the right, counting on
    /// the laps the low-tide run had left.
    /// </summary>
    internal static AnimationContinuation<P> HighTideLapsAfterRightLap { get; } =
        AnimationContinuation<P>.CountingOn(RunningAroundHighTide, RunningAroundHighTide.EndOfPlayOf(P.RunningAroundHighTideRight));

    /// <summary>Where the critters' escape takes a lap to the left: the run for the escape, from its pause.</summary>
    internal static AnimationContinuation<P> EscapeAfterLeftLap { get; } = new(RunningForEscape, AnimationPlace.StartOfAnimation);

    /// <summary>Where the critters' escape takes a lap to the right: the run for the escape, from its acceleration.</summary>
    internal static AnimationContinuation<P> EscapeAfterRightLap { get; } = new(RunningForEscapeAccelerating, AnimationPlace.StartOfAnimation);

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<P> Whole(P segment) => Repeated(segment, 1);

    /// <summary><paramref name="plays"/> plays of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<P> Repeated(P segment, int plays) => new(segment, 0, Segments.Beats(segment).Count, plays);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, EscapeDachoraCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, EscapeDachoraCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, EscapeDachoraCue> Cue(EscapeDachoraCue cue) => new AnimationBeat<EnemyFrameId, EscapeDachoraCue>.Event(cue);
}
