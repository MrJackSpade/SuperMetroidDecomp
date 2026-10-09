namespace SuperMetroid.Desktop;

/// <summary>Observes queue drains without changing PCM, buffering, or device pacing.</summary>
internal sealed class WaveOutQueueHealth
{
    /// <summary>Serializes observation updates with snapshot reads.</summary>
    private readonly object sync = new();
    /// <summary>Counts eligible pre-refill observations and the subset where no native headers remained queued.</summary>
    private long observations, emptyObservations;
    /// <summary>Tracks the lowest and highest eligible native queue depths; the minimum starts at a sentinel until sampled.</summary>
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

    /// <summary>Captures accumulated native queue measurements together with the current managed queue depth.</summary>
    /// <param name="managedQueued">Managed audio blocks currently waiting to be submitted.</param>
    /// <returns>A consistent snapshot; native minimum and maximum are absent before any eligible observation.</returns>
    internal WaveOutQueueHealthSnapshot Snapshot(int managedQueued)
    {
        lock (sync) return new(observations, emptyObservations, observations == 0 ? null : minimum,
            observations == 0 ? null : maximum, managedQueued);
    }
}

/// <summary>Immutable counters and queue depths captured by <see cref="WaveOutQueueHealth.Snapshot"/>.</summary>
/// <param name="RefillObservations">Number of non-preroll observations taken before an audio refill.</param>
/// <param name="EmptyBeforeRefill">Observations where all native headers had returned before refill.</param>
/// <param name="MinimumNativeQueued">Smallest native queue depth, absent when no eligible observation exists.</param>
/// <param name="MaximumNativeQueued">Largest native queue depth, absent when no eligible observation exists.</param>
/// <param name="ManagedQueued">Managed audio blocks waiting to be submitted when the snapshot was read.</param>
internal readonly record struct WaveOutQueueHealthSnapshot(long RefillObservations, long EmptyBeforeRefill,
    int? MinimumNativeQueued, int? MaximumNativeQueued, int ManagedQueued);
