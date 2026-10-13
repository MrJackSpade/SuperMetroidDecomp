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

/// <summary>Reopens a lost Windows endpoint without restarting emulation or replaying stale sound.</summary>
internal sealed class RecoveringAudioOutput : IHostAudioOutput
{
    /// <summary>Factory used for initial endpoint creation and later reopen attempts.</summary>
    private readonly Func<IHostAudioOutput> open;
    /// <summary>Monotonic millisecond clock controlling endpoint retry spacing.</summary>
    private readonly Func<long> clock;
    /// <summary>Current host endpoint, absent while an outage is being recovered.</summary>
    private IHostAudioOutput? output;
    /// <summary>Earliest clock value at which another reopen attempt is allowed.</summary>
    private long retryAt;
    /// <summary>Tracks an endpoint outage and final output disposal.</summary>
    private bool recovering, disposed;

    /// <summary>Creates an output wrapper with an injectable reopen factory and retry clock.</summary>
    internal RecoveringAudioOutput(Func<IHostAudioOutput> open, Func<long>? clock = null)
    {
        this.open = open;
        this.clock = clock ?? (() => Environment.TickCount64);
    }

    /// <summary>Reports the live endpoint's capacity, or permits a submission that can trigger reopening.</summary>
    public bool CanAcceptFrame => output?.CanAcceptFrame ?? true;
    /// <summary>Reports live queue health, or an empty queue while the endpoint is absent.</summary>
    public WaveOutQueueHealthSnapshot QueueHealth => output?.QueueHealth ?? new(0, 0, null, null, 0);

    /// <summary>Submits current PCM only when a live or newly reopened endpoint is available.</summary>
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

    /// <summary>Resets a live endpoint, retiring it when the device became unavailable.</summary>
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

    /// <summary>Disposes a lost endpoint and schedules a later reopen while discarding unheard PCM.</summary>
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

    /// <summary>Releases the current endpoint and prevents further submissions.</summary>
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        try { output?.Dispose(); }
        finally { output = null; }
    }
}
