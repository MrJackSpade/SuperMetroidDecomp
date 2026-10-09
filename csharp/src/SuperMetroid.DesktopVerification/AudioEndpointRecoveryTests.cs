using SuperMetroid.Desktop;

internal static partial class Program
{
    /// <summary>Exercises endpoint loss, cleanup failure, timed reopen, queue backpressure, and reset recovery using deterministic output fixtures.</summary>
    private static void VerifyAudioEndpointRecovery()
    {
        long now = 0;
        int opens = 0;
        var first = new EndpointFixture();
        var replacement = new EndpointFixture();
        using var output = new RecoveringAudioOutput(() => ++opens switch
        {
            1 => first,
            2 => throw new WaveOutDeviceException(6, "waveOutOpen"),
            _ => replacement,
        }, () => now);
        output.Submit([1, 2]);
        first.Failure = new WaveOutDeviceException(6, "waveOutPrepareHeader");
        first.DisposeFailure = true;
        ExpectEndpointFailure(() => output.Submit([3, 4]));
        Check(first.Disposals == 1 && output.CanAcceptFrame,
            "lost endpoint must be retired once without blocking further emulation");
        for (int frame = 0; frame < 60; frame++) output.Submit([5, 6]);
        Check(opens == 1, "lost output must not reopen on every emulated frame");
        now = 1000;
        ExpectEndpointFailure(() => output.Submit([7, 8]));
        Check(opens == 2, "missing endpoint must receive one timed retry");
        now = 1999;
        output.Submit([9, 10]);
        Check(opens == 2, "failed reopen must restart the retry interval");
        now = 2000;
        output.Submit([11, 12]);
        Check(opens == 3 && first.Frames.Single().SequenceEqual(new short[] { 1, 2 }) &&
            replacement.Frames.Single().SequenceEqual(new short[] { 11, 12 }),
            "reconnection must submit current PCM, never duplicate or replay outage frames");
        replacement.Accepting = false;
        Check(!output.CanAcceptFrame, "healthy output must retain bounded queue backpressure");
        replacement.Accepting = true;
        output.Reset();
        Check(replacement.Resets == 1, "normal pause must reset the current output");
        replacement.Failure = new WaveOutDeviceException(5, "waveOutReset");
        output.Reset();
        Check(replacement.Disposals == 1 && output.CanAcceptFrame,
            "endpoint loss while pausing must retire the device without a fatal reset error");

        var unrelated = new EndpointFixture { Failure = new WaveOutDeviceException(7, "waveOutPrepareHeader") };
        using var strict = new RecoveringAudioOutput(() => unrelated, () => now);
        bool propagated = false;
        try { strict.Submit([13, 14]); }
        catch (WaveOutDeviceException error) when (!error.EndpointUnavailable) { propagated = true; }
        Check(propagated && unrelated.Disposals == 0,
            "unexpected device errors must remain visible rather than being classified as endpoint loss");
        WaveOutAudioDevice? native = null;
        using (var live = new RecoveringAudioOutput(() => native = new WaveOutAudioDevice(48000, 2, 1600, volumePercent: 0)))
        {
            live.Submit(new short[1600]);
            native!.WaitForPendingSubmissions();
            live.Reset();
            live.Submit(new short[1600]);
            native!.WaitForPendingSubmissions();
            Check(native!.PreparedBufferCountForVerification == WaveOutAudioPolicy.PrerollSilenceBufferCount + 1,
                "recovery wrapper must preserve native startup and pause-reset preroll");
        }
        Console.WriteLine("Audio endpoint recovery: reported driver loss, cleanup failure, bounded retry, current PCM on reconnect, queue pacing and pause loss pass.");
    }

    /// <summary>Runs an operation and asserts that it reports an unavailable audio endpoint.</summary>
    /// <param name="action">Submission or reset operation expected to throw an endpoint-unavailable error.</param>
    private static void ExpectEndpointFailure(Action action)
    {
        bool failed = false;
        try { action(); }
        catch (WaveOutDeviceException error) when (error.EndpointUnavailable) { failed = true; }
        Check(failed, "original endpoint error must remain reportable");
    }

    /// <summary>In-memory host audio output that records submissions and can inject worker or disposal failures.</summary>
    private sealed class EndpointFixture : IHostAudioOutput
    {
        /// <summary>PCM buffers accepted by this endpoint, retained for reconnect assertions.</summary>
        internal readonly List<short[]> Frames = [];

        /// <summary>Optional exception raised by submission or reset to simulate device failure.</summary>
        internal Exception? Failure;

        /// <summary>Whether disposal raises a cleanup error after the endpoint has already failed.</summary>
        internal bool DisposeFailure;

        /// <summary>Whether another PCM frame can be accepted without violating queue backpressure.</summary>
        internal bool Accepting = true;

        /// <summary>Counts reset and disposal calls made by the recovery wrapper.</summary>
        internal int Disposals, Resets;

        /// <summary>Gets the fixture's configured bounded-queue acceptance state.</summary>
        public bool CanAcceptFrame => Accepting;

        /// <summary>Gets an empty queue-health snapshot for this synchronous in-memory endpoint.</summary>
        public WaveOutQueueHealthSnapshot QueueHealth => new(0, 0, null, null, 0);

        /// <summary>Records a PCM submission or throws the configured endpoint failure.</summary>
        /// <param name="samples">Interleaved PCM samples submitted by the audio wrapper.</param>
        public void Submit(ReadOnlySpan<short> samples)
        {
            if (Failure is not null) throw Failure;
            Frames.Add(samples.ToArray());
        }
        /// <summary>Records a reset or throws the configured endpoint failure.</summary>
        public void Reset()
        {
            if (Failure is not null) throw Failure;
            Resets++;
        }
        /// <summary>Counts endpoint retirement and optionally simulates an additional cleanup failure.</summary>
        public void Dispose()
        {
            Disposals++;
            if (DisposeFailure) throw new AggregateException("Previously reported worker failure", Failure!);
        }
    }
}
