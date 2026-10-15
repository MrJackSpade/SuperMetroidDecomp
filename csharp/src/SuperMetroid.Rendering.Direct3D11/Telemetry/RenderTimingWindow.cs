using SuperMetroid.Core;
namespace SuperMetroid.Rendering.Direct3D11.Telemetry;

/// <summary>Bounded rolling timings. Recording never allocates; readers sort their own copy outside the lock.</summary>
internal sealed class RenderTimingWindow
{
    /// <summary>Guards writes and copies of the rolling sample window.</summary>
    private readonly Lock sync = new();
    /// <summary>Ring buffer of post-warmup timings in milliseconds.</summary>
    private readonly double[] samples;
    /// <summary>Number of initial observations excluded from percentile calculations.</summary>
    private readonly int warmup;
    /// <summary>Number of valid samples currently retained in the ring.</summary>
    private int count;
    /// <summary>Ring slot overwritten by the next post-warmup observation.</summary>
    private int next;
    /// <summary>Total valid observations, including those excluded as warmup.</summary>
    private long observed;

    /// <summary>Allocates a bounded timing ring and sets the number of initial observations to discard.</summary>
    internal RenderTimingWindow(int capacity = RenderTelemetryLimits.DefaultHistoryCapacity, int warmup = 60)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        Ensure.AtLeastZero(warmup);
        samples = new double[capacity];
        this.warmup = warmup;
    }

    /// <summary>Rejects invalid timings and records finite, nonnegative samples after the warmup interval.</summary>
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

    /// <summary>Copies retained samples under lock, then calculates nearest-rank percentiles outside the lock.</summary>
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
