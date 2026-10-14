namespace SuperMetroid.Desktop;

/// <summary>Host PCM output contract; emulation and SPC state remain owned by the caller.</summary>
internal interface IHostAudioOutput : IDisposable
{
    /// <summary>Whether the endpoint queue can admit another emulation-frame PCM buffer.</summary>
    bool CanAcceptFrame { get; }
    /// <summary>Current queued-frame and endpoint-health diagnostics.</summary>
    WaveOutQueueHealthSnapshot QueueHealth { get; }
    /// <summary>Queues one ordered PCM frame for host playback.</summary>
    void Submit(ReadOnlySpan<short> samples);
    /// <summary>Clears endpoint playback state without changing emulation or SPC ownership.</summary>
    void Reset();
}
