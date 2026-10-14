using SuperMetroid.Desktop;

using static SuperMetroid.DesktopVerification.DesktopVerificationSupport;

namespace SuperMetroid.DesktopVerification;

/// <summary>Checks device-loss retirement, bounded reopen attempts, and current-frame audio delivery.</summary>
internal static class AudioEndpointRecoveryTests
{
    /// <summary>Exercises lost and restored waveOut endpoints, including pause reset and unrelated driver errors.</summary>
    internal static void VerifyAudioEndpointRecovery()
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
            (native ?? throw new InvalidOperationException("Verification requires native.")).WaitForPendingSubmissions();
            live.Reset();
            live.Submit(new short[1600]);
            native.WaitForPendingSubmissions();
            Check(native.PreparedBufferCountForVerification == WaveOutAudioPolicy.PrerollSilenceBufferCount + 1,
                "recovery wrapper must preserve native startup and pause-reset preroll");
        }
        Console.WriteLine("Audio endpoint recovery: reported driver loss, cleanup failure, bounded retry, current PCM on reconnect, queue pacing and pause loss pass.");
    }

    /// <summary>Requires the original endpoint-unavailable error to remain visible to the caller.</summary>
    private static void ExpectEndpointFailure(Action action)
    {
        bool failed = false;
        try { action(); }
        catch (WaveOutDeviceException error) when (error.EndpointUnavailable) { failed = true; }
        Check(failed, "original endpoint error must remain reportable");
    }

    /// <summary>Controllable host endpoint that records submitted PCM and retirement attempts.</summary>
    internal sealed class EndpointFixture : IHostAudioOutput
    {
        /// <summary>Copies of PCM frames accepted before any injected failure.</summary>
        internal readonly List<short[]> Frames = [];
        /// <summary>Injected submit or reset error, allowing endpoint loss and unrelated errors to be distinguished.</summary>
        internal Exception? Failure;
        /// <summary>Simulates a cleanup exception after the endpoint has already failed.</summary>
        internal bool DisposeFailure;
        /// <summary>Controls queue backpressure while the endpoint is otherwise healthy.</summary>
        internal bool Accepting = true;
        /// <summary>Counts disposal and reset calls so retirement occurs exactly once.</summary>
        internal int Disposals, Resets;
        /// <summary>Reflects the fixture's configurable queue acceptance state.</summary>
        public bool CanAcceptFrame => Accepting;
        /// <summary>Presents an empty healthy queue; backpressure is controlled separately.</summary>
        public WaveOutQueueHealthSnapshot QueueHealth => new(0, 0, null, null, 0);
        /// <summary>Copies submitted PCM unless an injected endpoint error occurs.</summary>
        public void Submit(ReadOnlySpan<short> samples)
        {
            if (Failure is not null) throw Failure;
            Frames.Add(samples.ToArray());
        }
        /// <summary>Records a pause reset unless the endpoint has been made unavailable.</summary>
        public void Reset()
        {
            if (Failure is not null) throw Failure;
            Resets++;
        }
        /// <summary>Counts retirement and optionally simulates a secondary cleanup failure.</summary>
        public void Dispose()
        {
            Disposals++;
            if (DisposeFailure) throw new AggregateException("Previously reported worker failure", (Failure ?? throw new InvalidOperationException("Verification requires Failure.")));
        }
    }
}
