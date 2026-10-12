namespace SuperMetroid.Desktop;

/// <summary>Host PCM output contract; emulation and SPC state remain owned by the caller.</summary>
internal interface IHostAudioOutput : IDisposable
{
    /// <summary>Gets can accept frame.</summary>
    bool CanAcceptFrame { get; }
    /// <summary>Gets queue health.</summary>
    WaveOutQueueHealthSnapshot QueueHealth { get; }
    /// <summary>submit.</summary>
    void Submit(ReadOnlySpan<short> samples);
    /// <summary>reset.</summary>
    void Reset();
}

/// <summary>Reopens a lost Windows endpoint without restarting emulation or replaying stale sound.</summary>
internal sealed class RecoveringAudioOutput : IHostAudioOutput
{
    /// <summary>Stores open.</summary>
    private readonly Func<IHostAudioOutput> open;
    /// <summary>Stores clock.</summary>
    private readonly Func<long> clock;
    /// <summary>Stores output.</summary>
    private IHostAudioOutput? output;
    /// <summary>Stores retry at.</summary>
    private long retryAt;
    /// <summary>Stores recovering.</summary>
    private bool recovering, disposed;

    /// <summary>Initializes a new recovering audio output instance.</summary>
    internal RecoveringAudioOutput(Func<IHostAudioOutput> open, Func<long>? clock = null)
    {
        this.open = open;
        this.clock = clock ?? (() => Environment.TickCount64);
    }

    /// <summary>Gets can accept frame.</summary>
    public bool CanAcceptFrame => output?.CanAcceptFrame ?? true;
    /// <summary>Gets queue health.</summary>
    public WaveOutQueueHealthSnapshot QueueHealth => output?.QueueHealth ?? new(0, 0, null, null, 0);

    /// <summary>submit.</summary>
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

    /// <summary>reset.</summary>
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

    /// <summary>retire output.</summary>
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

    /// <summary>Releases resources held by this instance.</summary>
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        try { output?.Dispose(); }
        finally { output = null; }
    }
}