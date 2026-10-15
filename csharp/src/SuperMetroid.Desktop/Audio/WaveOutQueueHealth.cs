using SuperMetroid.Core;
namespace SuperMetroid.Desktop.Audio;

/// <summary>Observes queue drains without changing PCM, buffering, or device pacing.</summary>
internal sealed class WaveOutQueueHealth
{
    /// <summary>Protects queue statistics shared by audio refill and diagnostic readers.</summary>
    private readonly Lock sync = new();
    /// <summary>Tracks refill samples and the subset where no native buffers remained queued.</summary>
    private long observations, emptyObservations;
    /// <summary>Bounds the observed native queue depth after startup preroll.</summary>
    private int minimum = int.MaxValue, maximum;

    /// <summary>
    /// Called by the audio owner before an emulated block is written. Startup/reset
    /// preroll is excluded. Zero means all native headers have returned before refill;
    /// this is an observed queue drain, not a hardware-reported underrun duration.
    /// </summary>
    internal void Observe(int hardwareQueued, bool prerollRequired)
    {
        Ensure.AtLeastZero(hardwareQueued);
        if (prerollRequired) return;
        lock (sync)
        {
            observations++;
            if (hardwareQueued == 0) emptyObservations++;
            minimum = Math.Min(minimum, hardwareQueued);
            maximum = Math.Max(maximum, hardwareQueued);
        }
    }

    /// <summary>Captures queue-depth statistics with the caller's current managed backlog.</summary>
    internal WaveOutQueueHealthSnapshot Snapshot(int managedQueued)
    {
        lock (sync) return new(observations, emptyObservations, observations == 0 ? null : minimum,
            observations == 0 ? null : maximum, managedQueued);
    }
}
