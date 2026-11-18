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
            .. Stride(DachoraStride.Left, 3, -LapStepPixels),
            Cue(new EscapeDachoraCue.LowTideLapEnds(Lap: HorizontalDirection.Left)),
            Cue(new EscapeDachoraCue.LapEnds(Lap: HorizontalDirection.Left)),
        ],
        [P.RunningAroundLowTideRight] =
        [
            .. Stride(DachoraStride.Right, 3, LapStepPixels),
            Cue(new EscapeDachoraCue.LowTideLapEnds(Lap: HorizontalDirection.Right)),
            Cue(new EscapeDachoraCue.LapEnds(Lap: HorizontalDirection.Right)),
        ],
        [P.RunningAroundHighTideLeft] =
        [
            .. Stride(DachoraStride.Left, 2, -LapStepPixels),
            Cue(new EscapeDachoraCue.LapEnds(Lap: HorizontalDirection.Left)),
        ],
        [P.RunningAroundHighTideRight] =
        [
            .. Stride(DachoraStride.Right, 2, LapStepPixels),
            Cue(new EscapeDachoraCue.LapEnds(Lap: HorizontalDirection.Right)),
        ],
        [P.RunningForEscape] =
        [
            Show(EnemyFrameId.DachoraRunningLeftFarLegBack, 30),
        ],
        [P.RunningForEscapeAccelerating] =
        [
            .. Stride(DachoraStride.Right, [90, 5, 5, 4, 4, 4], LapStepPixels),
            .. Stride(DachoraStride.Right, [3, 3, 3, 2, 2, 2], LapStepPixels),
        ],
        [P.RunningForEscapeMaximumSpeed] = Stride(DachoraStride.Right, 1, LapStepPixels),
    });

    /// <summary>The pixels the escaping Dachora steps with each pose of its stride, toward the way it runs.</summary>
    private const int LapStepPixels = 6;

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

    /// <summary>
    /// One <paramref name="stride"/> (#2477), each pose shown for <paramref name="updates"/> updates and followed by a step of
    /// <paramref name="step"/> pixels.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, EscapeDachoraCue>[] Stride(DachoraStride stride, int updates, int step) =>
        Stride(stride, [.. stride.Poses.Select(_ => updates)], step);

    /// <summary>
    /// One <paramref name="stride"/> whose pace changes from pose to pose (#2477): each pose shown for its own count of
    /// <paramref name="updates"/> and followed by a step of <paramref name="step"/> pixels.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="updates"/> does not time every pose of the stride.</exception>
    private static AnimationBeat<EnemyFrameId, EscapeDachoraCue>[] Stride(DachoraStride stride, IReadOnlyList<int> updates, int step) =>
        updates.Count == stride.Poses.Count
            ? [.. stride.Poses.SelectMany((pose, index) => new[] { Show(pose, updates[index]), Cue(new EscapeDachoraCue.Move(Offset: step)) })]
            : throw new ArgumentException($"A Dachora stride of {stride.Poses.Count} poses is timed by {updates.Count} counts.", nameof(updates));

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, EscapeDachoraCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, EscapeDachoraCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, EscapeDachoraCue> Cue(EscapeDachoraCue cue) => new AnimationBeat<EnemyFrameId, EscapeDachoraCue>.Event(cue);
}
