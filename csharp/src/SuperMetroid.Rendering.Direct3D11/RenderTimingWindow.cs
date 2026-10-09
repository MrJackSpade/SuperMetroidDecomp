namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>Bounded rolling timings. Recording never allocates; readers sort their own copy outside the lock.</summary>
internal sealed class RenderTimingWindow
{
    /// <summary>Protects sample writes and snapshots that copy the retained window.</summary>
    private readonly object sync = new();
    /// <summary>Fixed-capacity circular buffer of post-warmup duration samples in milliseconds.</summary>
    private readonly double[] samples;
    /// <summary>Number of initial records excluded before samples enter the rolling window.</summary>
    private readonly int warmup;
    /// <summary>Count is the retained sample total; next is the next circular-buffer write index.</summary>
    private int count, next;
    /// <summary>Total number of valid duration records received, including warmup records.</summary>
    private long observed;

    /// <summary>Creates a bounded timing window and configures how many initial records are excluded from retention.</summary>
    /// <param name="capacity">Maximum number of post-warmup samples retained for percentile calculations.</param>
    /// <param name="warmup">Number of initial records counted but excluded from the rolling sample buffer.</param>
    internal RenderTimingWindow(int capacity = RenderTelemetryLimits.DefaultHistoryCapacity, int warmup = 60)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        ArgumentOutOfRangeException.ThrowIfNegative(warmup);
        samples = new double[capacity];
        this.warmup = warmup;
    }

    /// <summary>Counts a timing observation and stores it in the rolling window after warmup completes.</summary>
    /// <param name="milliseconds">Finite, nonnegative duration to record.</param>
    /// <exception cref="ArgumentOutOfRangeException">The duration is negative or nonfinite.</exception>
    internal void Record(double milliseconds)
    {
        if (!double.IsFinite(milliseconds) || milliseconds < 0)
            throw new ArgumentOutOfRangeException(nameof(milliseconds));
        lock (sync)
        {
            if (++observed <= warmup) return;
            samples[next] = milliseconds;
            next = (next + 1) % samples.Length;
            count = Math.Min(count + 1, samples.Length);
        }
    }

    /// <summary>Copies and sorts retained samples to produce nearest-rank percentiles without holding the lock while sorting.</summary>
    /// <returns>Observed, warmup, retained-count, and p50/p95/p99 values; empty percentiles are <see cref="double.NaN"/>.</returns>
    internal RenderTimingDistribution Snapshot()
    {
        double[] copy;
        long total;
        lock (sync) { copy = samples.AsSpan(0, count).ToArray(); total = observed; }
        Array.Sort(copy);
        double Percentile(double percentile) => copy.Length == 0 ? double.NaN : copy[(int)Math.Ceiling(copy.Length * percentile) - 1];
        return new(total, Math.Min(total, warmup), copy.Length, Percentile(0.50), Percentile(0.95), Percentile(0.99));
    }
}

/// <summary>Nearest-rank milliseconds from the retained rolling window, not a whole-run soak summary.</summary>
/// <param name="Observed">Total accepted timing records, including excluded warmup records.</param>
/// <param name="WarmupExcluded">Number of initial records excluded from the retained sample set.</param>
/// <param name="Retained">Number of samples currently available to percentile calculation.</param>
/// <param name="P50Milliseconds">Nearest-rank median duration, or NaN when no samples are retained.</param>
/// <param name="P95Milliseconds">Nearest-rank 95th-percentile duration, or NaN when no samples are retained.</param>
/// <param name="P99Milliseconds">Nearest-rank 99th-percentile duration, or NaN when no samples are retained.</param>
public readonly record struct RenderTimingDistribution(long Observed, long WarmupExcluded, int Retained,
    double P50Milliseconds, double P95Milliseconds, double P99Milliseconds);

/// <summary>Independent windows: GPU results arrive asynchronously; Present includes display submission and host waiting.</summary>
/// <param name="CpuComposition">CPU-side time spent composing a frame before publication.</param>
/// <param name="CpuDisplayAndPresent">CPU display submission and Present duration, including host wait time.</param>
/// <param name="GpuComposition">GPU execution time for frame composition, excluding display and Present.</param>
/// <param name="GpuCompositionAndDisplay">GPU execution time including display work but excluding CPU Present wait.</param>
public readonly record struct RenderWorkerTimings(RenderTimingDistribution CpuComposition,
    RenderTimingDistribution CpuDisplayAndPresent, RenderTimingDistribution GpuComposition,
    RenderTimingDistribution GpuCompositionAndDisplay);
