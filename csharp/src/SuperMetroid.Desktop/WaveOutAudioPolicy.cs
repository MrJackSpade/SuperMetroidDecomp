namespace SuperMetroid.Desktop;

/// <summary>Named host buffering limits for the Windows PCM presentation boundary.</summary>
internal static class WaveOutAudioPolicy
{
    /// <summary>One-frame native buffers kept continuously queued to the waveOut device.</summary>
    public const int HardwareBufferCount = 6;

    /// <summary>
    /// Silent buffers queued before the first emulated PCM frame. Three frames provide
    /// 50 ms of host/RDP scheduling tolerance without making controller-driven sound feel
    /// detached from the corresponding video frame.
    /// </summary>
    public const int PrerollSilenceBufferCount = 3;

    /// <summary>
    /// Additional frames the worker may hold during brief Windows/RDP scheduling stalls.
    /// At 60 Hz this is 400 ms; filling it indicates a real producer/consumer failure.
    /// </summary>
    public const int ManagedQueueCapacityFrames = 24;

    /// <summary>Maximum time a functioning waveOut endpoint may retain every native buffer.</summary>
    public const int BufferReturnTimeoutMilliseconds = 2_000;
}
