namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>Bounded rolling timings. Recording never allocates; readers sort their own copy outside the lock.</summary>
internal sealed class RenderTimingWindow
{
    private readonly object sync = new();
    private readonly double[] samples;
    private readonly int warmup;
    private int count, next;
    private long observed;

    internal RenderTimingWindow(int capacity = 2048, int warmup = 60)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        ArgumentOutOfRangeException.ThrowIfNegative(warmup);
        samples = new double[capacity];
        this.warmup = warmup;
    }

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
public readonly record struct RenderTimingDistribution(long Observed, long WarmupExcluded, int Retained,
    double P50Milliseconds, double P95Milliseconds, double P99Milliseconds);

/// <summary>Independent windows: GPU results arrive asynchronously; Present includes display submission and host waiting.</summary>
public readonly record struct RenderWorkerTimings(RenderTimingDistribution CpuComposition,
    RenderTimingDistribution CpuDisplayAndPresent, RenderTimingDistribution GpuComposition,
    RenderTimingDistribution GpuCompositionAndDisplay);
