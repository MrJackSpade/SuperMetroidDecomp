using System.Diagnostics;
using System.Globalization;

namespace SuperMetroid.Desktop;

/// <summary>
/// One published interval of host-side timing data. Emulated FPS measures completed
/// cartridge frames; paint FPS measures completed WinForms canvas paints. Keeping the two
/// rates separate makes a slow translation distinguishable from a slow presentation path.
/// </summary>
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
    private readonly long timestampFrequency;
    private readonly long reportingIntervalTicks;
    private long intervalStarted;
    private long emulationTicks;
    private long worstEmulationTicks;
    private long paintTicks;
    private long worstPaintTicks;
    private int emulatedFrames;
    private int paintedFrames;
    private double lateFrames;

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

    private double ToAverageMilliseconds(long ticks, int observations) =>
        observations == 0 ? 0 : ToMilliseconds(ticks) / observations;

    private double ToMilliseconds(long ticks) => ticks * 1000.0 / timestampFrequency;

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
