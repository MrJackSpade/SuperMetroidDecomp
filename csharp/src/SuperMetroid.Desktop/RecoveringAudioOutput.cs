namespace SuperMetroid.Desktop;

/// <summary>Host PCM output contract; emulation and SPC state remain owned by the caller.</summary>
internal interface IHostAudioOutput : IDisposable
{
    /// <summary>Indicates whether one emulated frame's PCM can be accepted by the host endpoint.</summary>
    bool CanAcceptFrame { get; }
    /// <summary>Reports buffered-frame and underrun health for the active endpoint.</summary>
    WaveOutQueueHealthSnapshot QueueHealth { get; }
    /// <summary>Submits generated PCM samples to the host audio endpoint.</summary>
    /// <param name="samples">Interleaved signed PCM samples for the current emulation output.</param>
    void Submit(ReadOnlySpan<short> samples);
    /// <summary>Discards queued host audio and resets endpoint playback state.</summary>
    void Reset();
}

/// <summary>Reopens a lost Windows endpoint without restarting emulation or replaying stale sound.</summary>
internal sealed class RecoveringAudioOutput : IHostAudioOutput
{
    /// <summary>Factory used to create or reopen the underlying Windows audio endpoint.</summary>
    private readonly Func<IHostAudioOutput> open;
    /// <summary>Monotonic time source used to schedule endpoint reopen attempts.</summary>
    private readonly Func<long> clock;
    /// <summary>Current endpoint, or null while recovery is waiting for its next retry.</summary>
    private IHostAudioOutput? output;
    /// <summary>Earliest clock tick at which another endpoint open is attempted.</summary>
    private long retryAt;
    /// <summary>Tracks endpoint-recovery reporting and whether this wrapper has been disposed.</summary>
    private bool recovering, disposed;

    /// <summary>Creates a lazy audio-output wrapper that can reopen the endpoint after device loss.</summary>
    /// <param name="open">Factory for the underlying host audio output.</param>
    /// <param name="clock">Optional monotonic tick source used for retry throttling.</param>
    internal RecoveringAudioOutput(Func<IHostAudioOutput> open, Func<long>? clock = null)
    {
        this.open = open;
        this.clock = clock ?? (() => Environment.TickCount64);
    }

    /// <summary>Gets the endpoint's capacity, allowing the caller to continue while recovery is pending.</summary>
    public bool CanAcceptFrame => output?.CanAcceptFrame ?? true;
    /// <summary>Gets current queue health, or an empty snapshot when no endpoint is open.</summary>
    public WaveOutQueueHealthSnapshot QueueHealth => output?.QueueHealth ?? new(0, 0, null, null, 0);

    /// <summary>Submits samples to the endpoint, reopening it when the retry interval permits.</summary>
    /// <param name="samples">Interleaved signed PCM samples; samples lost during an outage are not replayed.</param>
    public void Submit(ReadOnlySpan<short> samples)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (output is null && recovering && clock() < retryAt) return;
        try
        {
            if (output is null)
            {
                output = open();
                if (recovering) Console.Error.WriteLine("Audio output reopened; resuming current sound without replaying the outage.");
                recovering = false;
            }
            output.Submit(samples);
        }
        catch (WaveOutDeviceException error) when (error.EndpointUnavailable)
        {
            RetireOutput();
            throw; // The existing audio recovery boundary records the original operation/code.
        }
    }

    /// <summary>Resets the active endpoint and retires it if the device has become unavailable.</summary>
    public void Reset()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        try { output?.Reset(); }
        catch (WaveOutDeviceException error) when (error.EndpointUnavailable)
        {
            Console.Error.WriteLine(error.ToString());
            RetireOutput();
        }
    }

    /// <summary>Closes the failed endpoint and schedules a later attempt to reopen it.</summary>
    private void RetireOutput()
    {
        IHostAudioOutput? failed = output;
        output = null;
        retryAt = clock() + WaveOutAudioPolicy.DeviceReopenIntervalMilliseconds;
        if (!recovering)
            Console.Error.WriteLine("Audio endpoint unavailable; gameplay/SPC continue, unheard PCM is discarded, and output will be retried once per second.");
        recovering = true;
        // The worker must stop before reopening. Its original failure is also included
        // in Dispose's aggregate, so cleanup diagnostics must not block the retry.
        try { failed?.Dispose(); }
        catch (Exception error) when (error is not OutOfMemoryException)
        {
            Console.Error.WriteLine("Audio endpoint cleanup reported:");
            Console.Error.WriteLine(error.ToString());
        }
    }

    /// <summary>Disposes the wrapper and its current endpoint; repeated calls have no effect.</summary>
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        try { output?.Dispose(); }
        finally { output = null; }
    }
}
