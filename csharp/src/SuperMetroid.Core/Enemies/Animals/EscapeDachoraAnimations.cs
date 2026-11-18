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
            Show(EnemyFrameId.DachoraRunningLeftFarLegBack, 3),
            Cue(new EscapeDachoraCue.Move(Offset: -6)),
            Show(EnemyFrameId.DachoraRunningLeftFarLegPassing, 3),
            Cue(new EscapeDachoraCue.Move(Offset: -6)),
            Show(EnemyFrameId.DachoraRunningLeftFarLegReaching, 3),
            Cue(new EscapeDachoraCue.Move(Offset: -6)),
            Show(EnemyFrameId.DachoraRunningLeftNearLegBack, 3),
            Cue(new EscapeDachoraCue.Move(Offset: -6)),
            Show(EnemyFrameId.DachoraRunningLeftNearLegPassing, 3),
            Cue(new EscapeDachoraCue.Move(Offset: -6)),
            Show(EnemyFrameId.DachoraRunningLeftNearLegReaching, 3),
            Cue(new EscapeDachoraCue.Move(Offset: -6)),
            Cue(new EscapeDachoraCue.LowTideLapEnds(Lap: HorizontalDirection.Left)),
            Cue(new EscapeDachoraCue.LapEnds(Lap: HorizontalDirection.Left)),
        ],
        [P.RunningAroundLowTideRight] =
        [
            Show(EnemyFrameId.DachoraRunningRightFarLegBack, 3),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.DachoraRunningRightFarLegPassing, 3),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.DachoraRunningRightFarLegReaching, 3),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.DachoraRunningRightNearLegBack, 3),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.DachoraRunningRightNearLegPassing, 3),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.DachoraRunningRightNearLegReaching, 3),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Cue(new EscapeDachoraCue.LowTideLapEnds(Lap: HorizontalDirection.Right)),
            Cue(new EscapeDachoraCue.LapEnds(Lap: HorizontalDirection.Right)),
        ],
        [P.RunningAroundHighTideLeft] =
        [
            Show(EnemyFrameId.DachoraRunningLeftFarLegBack, 2),
            Cue(new EscapeDachoraCue.Move(Offset: -6)),
            Show(EnemyFrameId.DachoraRunningLeftFarLegPassing, 2),
            Cue(new EscapeDachoraCue.Move(Offset: -6)),
            Show(EnemyFrameId.DachoraRunningLeftFarLegReaching, 2),
            Cue(new EscapeDachoraCue.Move(Offset: -6)),
            Show(EnemyFrameId.DachoraRunningLeftNearLegBack, 2),
            Cue(new EscapeDachoraCue.Move(Offset: -6)),
            Show(EnemyFrameId.DachoraRunningLeftNearLegPassing, 2),
            Cue(new EscapeDachoraCue.Move(Offset: -6)),
            Show(EnemyFrameId.DachoraRunningLeftNearLegReaching, 2),
            Cue(new EscapeDachoraCue.Move(Offset: -6)),
            Cue(new EscapeDachoraCue.LapEnds(Lap: HorizontalDirection.Left)),
        ],
        [P.RunningAroundHighTideRight] =
        [
            Show(EnemyFrameId.DachoraRunningRightFarLegBack, 2),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.DachoraRunningRightFarLegPassing, 2),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.DachoraRunningRightFarLegReaching, 2),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.DachoraRunningRightNearLegBack, 2),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.DachoraRunningRightNearLegPassing, 2),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.DachoraRunningRightNearLegReaching, 2),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Cue(new EscapeDachoraCue.LapEnds(Lap: HorizontalDirection.Right)),
        ],
        [P.RunningForEscape] =
        [
            Show(EnemyFrameId.DachoraRunningLeftFarLegBack, 30),
        ],
        [P.RunningForEscapeAccelerating] =
        [
            Show(EnemyFrameId.DachoraRunningRightFarLegBack, 90),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.DachoraRunningRightFarLegPassing, 5),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.DachoraRunningRightFarLegReaching, 5),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.DachoraRunningRightNearLegBack, 4),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.DachoraRunningRightNearLegPassing, 4),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.DachoraRunningRightNearLegReaching, 4),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.DachoraRunningRightFarLegBack, 3),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.DachoraRunningRightFarLegPassing, 3),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.DachoraRunningRightFarLegReaching, 3),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.DachoraRunningRightNearLegBack, 2),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.DachoraRunningRightNearLegPassing, 2),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.DachoraRunningRightNearLegReaching, 2),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
        ],
        [P.RunningForEscapeMaximumSpeed] =
        [
            Show(EnemyFrameId.DachoraRunningRightFarLegBack, 1),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.DachoraRunningRightFarLegPassing, 1),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.DachoraRunningRightFarLegReaching, 1),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.DachoraRunningRightNearLegBack, 1),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.DachoraRunningRightNearLegPassing, 1),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
            Show(EnemyFrameId.DachoraRunningRightNearLegReaching, 1),
            Cue(new EscapeDachoraCue.Move(Offset: 6)),
        ],
    });

    /// <summary>How many laps the escaping Dachora runs each way before it turns around.</summary>
    private const int LapsEachWay = 5;

    /// <summary>Five laps left then five right at low tide, each lap counted, over and over (#2061).</summary>
    internal static Animation<P> RunningAroundLowTide { get; } =
        Animation<P>.Looping([Segments.Repeated(P.RunningAroundLowTideLeft, LapsEachWay), Segments.Repeated(P.RunningAroundLowTideRight, LapsEachWay)]);

    /// <summary>Five quicker laps left then five right at high tide, each lap counted, over and over (#2061).</summary>
    internal static Animation<P> RunningAroundHighTide { get; } =
        Animation<P>.Looping([Segments.Repeated(P.RunningAroundHighTideLeft, LapsEachWay), Segments.Repeated(P.RunningAroundHighTideRight, LapsEachWay)]);

    /// <summary>The pause, the acceleration, then the sprint at full speed for good.</summary>
    internal static Animation<P> RunningForEscape { get; } = Animation<P>.LeadingInto(
        [Segments.Whole(P.RunningForEscape), Segments.Whole(P.RunningForEscapeAccelerating)], [Segments.Whole(P.RunningForEscapeMaximumSpeed)]);

    /// <summary>The acceleration, then the sprint at full speed for good.</summary>
    internal static Animation<P> RunningForEscapeAccelerating { get; } = Animation<P>.LeadingInto(
        [Segments.Whole(P.RunningForEscapeAccelerating)], [Segments.Whole(P.RunningForEscapeMaximumSpeed)]);

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

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, EscapeDachoraCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, EscapeDachoraCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, EscapeDachoraCue> Cue(EscapeDachoraCue cue) => new AnimationBeat<EnemyFrameId, EscapeDachoraCue>.Event(cue);
}
