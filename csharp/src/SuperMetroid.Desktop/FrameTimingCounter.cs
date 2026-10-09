using System.Diagnostics;
using System.Globalization;

namespace SuperMetroid.Desktop;

/// <summary>
/// One published interval of host-side timing data. Emulated FPS measures completed
/// cartridge frames; paint FPS measures completed WinForms canvas paints. Keeping the two
/// rates separate makes a slow translation distinguishable from a slow presentation path.
/// </summary>
/// <param name="EmulatedFramesPerSecond">Completed emulated frames divided by the interval's elapsed wall time.</param>
/// <param name="PaintedFramesPerSecond">Completed canvas paints divided by the interval's elapsed wall time.</param>
/// <param name="AverageEmulationMilliseconds">Mean host time spent advancing one emulated frame during the interval.</param>
/// <param name="WorstEmulationMilliseconds">Longest measured host time for a single emulated frame during the interval.</param>
/// <param name="AveragePaintMilliseconds">Mean host time spent painting one canvas frame during the interval.</param>
/// <param name="WorstPaintMilliseconds">Longest measured canvas paint duration during the interval.</param>
/// <param name="LateFrames">Fractional count of wall-clock frames discarded by catch-up limits.</param>
public readonly record struct FrameTimingSnapshot(
    double EmulatedFramesPerSecond,
    double PaintedFramesPerSecond,
    double AverageEmulationMilliseconds,
    double WorstEmulationMilliseconds,
    double AveragePaintMilliseconds,
    double WorstPaintMilliseconds,
    double LateFrames)
{
    /// <summary>Compact text intended for the fixed-width gameplay toolbar.</summary>
    public string ToToolbarText() => string.Create(
        CultureInfo.InvariantCulture,
        $"emu {EmulatedFramesPerSecond,4:F1} | paint {PaintedFramesPerSecond,4:F1} | " +
        $"step {AverageEmulationMilliseconds:F1}/{WorstEmulationMilliseconds:F1} ms | " +
        $"late {LateFrames:F1}");

    /// <summary>Expanded explanation retained in the status tooltip.</summary>
    public string ToDiagnosticText() => string.Create(
        CultureInfo.InvariantCulture,
        $"Emulation {EmulatedFramesPerSecond:F2} fps, canvas paint {PaintedFramesPerSecond:F2} fps; " +
        $"emulation frame average/worst {AverageEmulationMilliseconds:F2}/{WorstEmulationMilliseconds:F2} ms; " +
        $"paint average/worst {AveragePaintMilliseconds:F2}/{WorstPaintMilliseconds:F2} ms; " +
        $"discarded late frames {LateFrames:F2}");
}

/// <summary>
/// Accumulates timing observations without allocating or consulting game state. All calls
/// are made by the WinForms UI thread, so snapshots need no locks and cannot perturb the
/// emulation worker with cross-thread synchronization.
/// </summary>
public sealed class FrameTimingCounter
{
    /// <summary>Optional owner-thread diagnostic observer; the normal host has no subscriber.</summary>
    internal event Action<long>? EmulatedFrameMeasured;
    /// <summary>Owner-thread observer for whole-run catch-up discards, independent of toolbar intervals.</summary>
    internal event Action<double>? LateFramesRecorded;
    /// <summary>Clock ticks per second used to convert elapsed measurements into wall time.</summary>
    private readonly long timestampFrequency;
    /// <summary>Minimum elapsed clock ticks required before an interval can be published.</summary>
    private readonly long reportingIntervalTicks;
    /// <summary>Timestamp at which the current reporting interval began or was last published.</summary>
    private long intervalStarted;
    /// <summary>Sum of host ticks spent advancing emulated frames in the current interval.</summary>
    private long emulationTicks;
    /// <summary>Longest single emulated-frame duration recorded in the current interval.</summary>
    private long worstEmulationTicks;
    /// <summary>Sum of host ticks spent painting frames in the current interval.</summary>
    private long paintTicks;
    /// <summary>Longest single canvas-paint duration recorded in the current interval.</summary>
    private long worstPaintTicks;
    /// <summary>Number of emulated frames completed in the current interval.</summary>
    private int emulatedFrames;
    /// <summary>Number of canvas paints completed in the current interval.</summary>
    private int paintedFrames;
    /// <summary>Accumulated fractional catch-up frames discarded since the last snapshot.</summary>
    private double lateFrames;

    /// <summary>Creates an empty UI-thread counter using Stopwatch ticks and the host's one-second reporting interval.</summary>
    public FrameTimingCounter()
        : this(Stopwatch.Frequency, FrameTimingConfiguration.ReportingInterval)
    {
    }

    /// <summary>Injectable clock geometry used by the deterministic timing smoke test.</summary>
    public FrameTimingCounter(long timestampFrequency, TimeSpan reportingInterval)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(timestampFrequency);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(reportingInterval, TimeSpan.Zero);

        this.timestampFrequency = timestampFrequency;
        reportingIntervalTicks = Math.Max(
            1,
            checked((long)Math.Round(reportingInterval.TotalSeconds * timestampFrequency)));
    }

    /// <summary>Starts a clean reporting interval at the supplied Stopwatch timestamp.</summary>
    public void Reset(long timestamp)
    {
        intervalStarted = timestamp;
        ClearObservations();
    }

    /// <summary>Records the complete host cost of one translated video frame.</summary>
    public void RecordEmulatedFrame(long elapsedTicks)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(elapsedTicks);
        emulatedFrames++;
        emulationTicks += elapsedTicks;
        EmulatedFrameMeasured?.Invoke(elapsedTicks);
        worstEmulationTicks = Math.Max(worstEmulationTicks, elapsedTicks);
    }

    /// <summary>Records one canvas paint, including scaling the native raster to the host.</summary>
    public void RecordPaint(long elapsedTicks)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(elapsedTicks);
        paintedFrames++;
        paintTicks += elapsedTicks;
        worstPaintTicks = Math.Max(worstPaintTicks, elapsedTicks);
    }

    /// <summary>
    /// Records wall-clock frames deliberately discarded by the bounded catch-up policy.
    /// A nonzero value means the host stopped servicing gameplay long enough to fall behind.
    /// </summary>
    public void RecordLateFrames(double count)
    {
        if (!double.IsFinite(count) || count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));
        lateFrames += count;
        LateFramesRecorded?.Invoke(count);
    }

    /// <summary>Publishes and clears one interval once enough wall time has elapsed.</summary>
    public bool TryTakeSnapshot(long timestamp, out FrameTimingSnapshot snapshot)
    {
        long elapsedTicks = timestamp - intervalStarted;
        if (elapsedTicks < reportingIntervalTicks)
        {
            snapshot = default;
            return false;
        }
        if (elapsedTicks <= 0)
            throw new ArgumentOutOfRangeException(nameof(timestamp));

        double elapsedSeconds = (double)elapsedTicks / timestampFrequency;
        snapshot = new FrameTimingSnapshot(
            emulatedFrames / elapsedSeconds,
            paintedFrames / elapsedSeconds,
            ToAverageMilliseconds(emulationTicks, emulatedFrames),
            ToMilliseconds(worstEmulationTicks),
            ToAverageMilliseconds(paintTicks, paintedFrames),
            ToMilliseconds(worstPaintTicks),
            lateFrames);

        intervalStarted = timestamp;
        ClearObservations();
        return true;
    }

    /// <summary>Converts an accumulated tick duration to mean milliseconds per observation.</summary>
    /// <param name="ticks">Total clock ticks across the observations.</param>
    /// <param name="observations">Number of measured events represented by <paramref name="ticks"/>.</param>
    /// <returns>Average milliseconds, or zero when no events were recorded.</returns>
    private double ToAverageMilliseconds(long ticks, int observations) =>
        observations == 0 ? 0 : ToMilliseconds(ticks) / observations;

    /// <summary>Converts clock ticks to milliseconds using the injected or Stopwatch clock frequency.</summary>
    /// <param name="ticks">Elapsed clock ticks.</param>
    /// <returns>The equivalent duration in milliseconds.</returns>
    private double ToMilliseconds(long ticks) => ticks * 1000.0 / timestampFrequency;

    /// <summary>Clears interval-local frame counts, duration totals, maxima, and discarded-frame observations.</summary>
    private void ClearObservations()
    {
        emulationTicks = 0;
        worstEmulationTicks = 0;
        paintTicks = 0;
        worstPaintTicks = 0;
        emulatedFrames = 0;
        paintedFrames = 0;
        lateFrames = 0;
    }
}

/// <summary>Host timing policy kept separate from the counter's functional state.</summary>
internal static class FrameTimingConfiguration
{
    /// <summary>Refresh often enough to see stalls without making the numbers unreadably noisy.</summary>
    public static readonly TimeSpan ReportingInterval = TimeSpan.FromSeconds(1);
}
