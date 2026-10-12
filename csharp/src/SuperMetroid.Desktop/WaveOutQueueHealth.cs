using SuperMetroid.Core;
namespace SuperMetroid.Desktop;

/// <summary>Observes queue drains without changing PCM, buffering, or device pacing.</summary>
internal sealed class WaveOutQueueHealth
{
    /// <summary>Stores sync.</summary>
    private readonly Lock sync = new();
    /// <summary>Stores observations.</summary>
    private long observations, emptyObservations;
    /// <summary>Stores minimum.</summary>
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

    /// <summary>Snapshot.</summary>
    internal WaveOutQueueHealthSnapshot Snapshot(int managedQueued)
    {
        lock (sync) return new(observations, emptyObservations, observations == 0 ? null : minimum,
            observations == 0 ? null : maximum, managedQueued);
    }
}

/// <summary>Represents wave out queue health snapshot.</summary>
internal readonly record struct WaveOutQueueHealthSnapshot(long RefillObservations, long EmptyBeforeRefill,
    int? MinimumNativeQueued, int? MaximumNativeQueued, int ManagedQueued);
