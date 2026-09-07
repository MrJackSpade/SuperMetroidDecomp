namespace SuperMetroid.Desktop;

/// <summary>Observes queue drains without changing PCM, buffering, or device pacing.</summary>
internal sealed class WaveOutQueueHealth
{
    private readonly object sync = new();
    private long observations, emptyObservations;
    private int minimum = int.MaxValue, maximum;

    /// <summary>
    /// Called by the audio owner before an emulated block is written. Startup/reset
    /// preroll is excluded. Zero means all native headers have returned before refill;
    /// this is an observed queue drain, not a hardware-reported underrun duration.
    /// </summary>
    internal void Observe(int hardwareQueued, bool prerollRequired)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(hardwareQueued);
        if (prerollRequired) return;
        lock (sync)
        {
            observations++;
            if (hardwareQueued == 0) emptyObservations++;
            minimum = Math.Min(minimum, hardwareQueued);
            maximum = Math.Max(maximum, hardwareQueued);
        }
    }

    internal WaveOutQueueHealthSnapshot Snapshot(int managedQueued)
    {
        lock (sync) return new(observations, emptyObservations, observations == 0 ? null : minimum,
            observations == 0 ? null : maximum, managedQueued);
    }
}

internal readonly record struct WaveOutQueueHealthSnapshot(long RefillObservations, long EmptyBeforeRefill,
    int? MinimumNativeQueued, int? MaximumNativeQueued, int ManagedQueued);
