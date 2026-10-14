namespace SuperMetroid.Desktop;

/// <summary>One audio queue health reading, separating native refill observations from managed backlog.</summary>
internal readonly record struct WaveOutQueueHealthSnapshot(long RefillObservations, long EmptyBeforeRefill,
    int? MinimumNativeQueued, int? MaximumNativeQueued, int ManagedQueued);
