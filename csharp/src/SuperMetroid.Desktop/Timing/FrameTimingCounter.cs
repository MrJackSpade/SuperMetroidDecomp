using SuperMetroid.Core;
using System.Diagnostics;

namespace SuperMetroid.Desktop.Timing;

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
    /// <summary>Stopwatch ticks per second for converting observations to wall time.</summary>
    private readonly long timestampFrequency;
    /// <summary>Minimum Stopwatch ticks between published snapshots.</summary>
    private readonly long reportingIntervalTicks;
    /// <summary>Timestamp from which the current reporting interval is measured.</summary>
    private long intervalStarted;
    /// <summary>Total host ticks spent translating frames in this interval.</summary>
    private long emulationTicks;
    /// <summary>Longest single frame translation in this interval.</summary>
    private long worstEmulationTicks;
    /// <summary>Total host ticks spent painting the canvas in this interval.</summary>
    private long paintTicks;
    /// <summary>Longest canvas paint in this interval.</summary>
    private long worstPaintTicks;
    /// <summary>Completed translated frames in this interval.</summary>
    private int emulatedFrames;
    /// <summary>Completed canvas paints in this interval.</summary>
    private int paintedFrames;
    /// <summary>Wall-clock frame equivalents discarded by bounded catch-up in this interval.</summary>
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
        Ensure.AtLeastZero(elapsedTicks);
        emulatedFrames++;
        emulationTicks += elapsedTicks;
        EmulatedFrameMeasured?.Invoke(elapsedTicks);
        worstEmulationTicks = Math.Max(worstEmulationTicks, elapsedTicks);
    }

    /// <summary>Records one canvas paint, including scaling the native raster to the host.</summary>
    public void RecordPaint(long elapsedTicks)
    {
        Ensure.AtLeastZero(elapsedTicks);
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

    /// <summary>Converts an accumulated tick cost to an average, returning zero for an empty interval.</summary>
    private double ToAverageMilliseconds(long ticks, int observations) =>
        observations == 0 ? 0 : ToMilliseconds(ticks) / observations;

    /// <summary>Converts Stopwatch ticks using this counter's injected clock frequency.</summary>
    private double ToMilliseconds(long ticks) => ticks * 1000.0 / timestampFrequency;

    /// <summary>Clears interval counters while retaining the clock geometry and interval start.</summary>
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
