namespace SuperMetroid.Rendering.Direct3D11.Telemetry;

/// <summary>Nearest-rank milliseconds from the retained rolling window, not a whole-run soak summary.</summary>
public readonly record struct RenderTimingDistribution(long Observed, long WarmupExcluded, int Retained,
    double P50Milliseconds, double P95Milliseconds, double P99Milliseconds);
