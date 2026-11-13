namespace SuperMetroid.Desktop.Audio;

/// <summary>Host PCM output contract; the game and its sound state remain owned by the caller.</summary>
internal interface IHostAudioOutput : IDisposable
{
    /// <summary>Whether the endpoint queue can admit another game update's PCM buffer.</summary>
    bool CanAcceptFrame { get; }
    /// <summary>Current queued-frame and endpoint-health diagnostics.</summary>
    WaveOutQueueHealthSnapshot QueueHealth { get; }
    /// <summary>Queues one ordered PCM frame for host playback.</summary>
    void Submit(ReadOnlySpan<short> samples);
    /// <summary>Clears endpoint playback state without changing the game or its sound state.</summary>
    void Reset();
}
